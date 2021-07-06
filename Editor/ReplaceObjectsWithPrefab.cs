using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.FPS.EditorExt
{
    public class ReplaceObjectsWithPrefabs : EditorWindow
    {
        GameObject m_ParentObject;
        string m_DatabasePath = "";

        [MenuItem("Tools/ObjectsToPrefabs")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ReplaceObjectsWithPrefabs));
        }

        private void OnGUI()
        {
            m_ParentObject = EditorGUILayout.ObjectField("Parent Object", m_ParentObject, typeof(GameObject), true) as GameObject;

            m_DatabasePath = EditorGUILayout.TextField("Databse Path", m_DatabasePath);

            if (GUILayout.Button("Replace!"))
            {
                ReplaceObjects();
            }
        }

        private void ReplaceObjects()
        {
            List<GameObject> prefabs = new List<GameObject>();
            List<GameObject> childern = new List<GameObject>();

            for (int i = 0; i < m_ParentObject.transform.childCount; i++)
            {
                GameObject child = m_ParentObject.transform.GetChild(i).gameObject;
                // If the child is already a prefab skip it
                if (PrefabUtility.IsPartOfPrefabAsset(child))
                {
                    continue;
                }

                // Seeing as our prefab names doesn't include spaces we can split the scene objects by spaces and use the first part
                string[] formattedName = child.name.Split(' ');
                // Try to load the prefab at the path
                GameObject prefab = AssetDatabase.LoadAssetAtPath(m_DatabasePath + formattedName[0] + ".prefab", typeof(GameObject)) as GameObject;
                if (prefab != null)
                {
                    // Spawn in the prefab and set the same params as the child
                    GameObject spawnedInPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    spawnedInPrefab.transform.SetPositionAndRotation(child.transform.position, child.transform.rotation);
                    spawnedInPrefab.transform.localScale = child.transform.localScale;

                    // Add the prefab to the prefab list and the child to the child list
                    prefabs.Add(spawnedInPrefab);
                    childern.Add(child);
                }       
            }

            // Destroy each child we processed and replaced with a prefab
            foreach (GameObject child in childern)
            {
                DestroyImmediate(child);
            }

            // Set all the spawned in prefabs as child to the parent object
            foreach (GameObject prefab in prefabs)
            {
                prefab.transform.SetParent(m_ParentObject.transform);
            }
        }
    }

}