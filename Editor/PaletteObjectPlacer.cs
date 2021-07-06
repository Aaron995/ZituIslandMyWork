using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.FPS.EditorExt
{
    public struct ObjectInfo
    {
        public GameObject Prefab;
        public Editor PreviewEditor;
        public string Path;
        public ObjectInfo(GameObject prefab, string path)
        {
            Prefab = prefab;
            PreviewEditor = Editor.CreateEditor(prefab);
            PreviewEditor.HasPreviewGUI();

            Path = path;
        }
    }

    public enum SelectedPrefabTypeEnum 
    {
        Rock,
        Folliage
    }

    public class PaletteObjectPlacer : EditorWindow
    {
        List<ObjectInfo> m_RockPrefabs;
        List<ObjectInfo> m_FolliagePrefabs;

        bool m_ObjectsLoaded = false;
        bool m_ObjectsRefreshing = false;
        bool m_OpenRock = false;
        bool m_OpenFolliage = false;
        bool m_RandomXRotation = false;
        bool m_RandomYRotation = false;
        bool m_RandomZRotation = false;
        bool m_ClearOnPlace = true;

        string m_RockDrawerName = "Rocks";
        string m_FolliageDrawerName = "Folliage";

        Vector2 m_ScrollPosition;

        GameObject m_SelectedPrefab;
        SelectedPrefabTypeEnum m_SelectedPrefabType;

        [MenuItem("Tools/PaletteObjectPlacer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PaletteObjectPlacer));
        }

        private void OnDestroy()
        {
            // Destroy the Editor classes in each list
            if (m_RockPrefabs != null)
            {
                foreach (ObjectInfo obj in m_RockPrefabs)
                {
                    DestroyImmediate(obj.PreviewEditor,false);
                }
            }

            if (m_FolliagePrefabs != null)
            {
                foreach (ObjectInfo obj in m_FolliagePrefabs)
                {
                    DestroyImmediate(obj.PreviewEditor, false);
                }
            }

            SceneView.duringSceneGui -= SceneGUI;
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += SceneGUI;
        }

        private void OnGUI()
        {
            DrawerLabels();
            GUILayout.Space(20f);

            // A button to refresh the objects, incase something breaks or certain objects aren't showing up.
            if (GUILayout.Button("Refresh"))
            {
                m_ObjectsRefreshing = true;
                ReloadObjects();
            }
            GUILayout.Space(20f);          

            if (m_SelectedPrefab != null)
            {
                IfObjectSelected();
            }

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, false, true);

            if ((!m_ObjectsLoaded || m_RockPrefabs == null) && Event.current.type == EventType.Repaint)
            {
                // Load in the objects needed during repaint and on first time or if the list(s) are null.
                LoadObjects();
            }
            else if (!m_ObjectsRefreshing && m_RockPrefabs != null && m_FolliagePrefabs != null)
            {
                m_OpenRock =  EditorGUILayout.Foldout(m_OpenRock, "Rocks");

                if (m_OpenRock)
                {
                    DrawList(m_RockPrefabs);
                }

                m_OpenFolliage = EditorGUILayout.Foldout(m_OpenFolliage, "Folliage");

                if (m_OpenFolliage)
                {
                    DrawList(m_FolliagePrefabs);
                }

            }
            GUILayout.EndScrollView();
        }

        private void LoadObjects()
        {
            // Since the database is wonky at times we just get all asset paths, makes it easier to add more objects in the feature.
            string[] assetsPaths = AssetDatabase.GetAllAssetPaths();

            m_RockPrefabs = new List<ObjectInfo>();
            m_FolliagePrefabs = new List<ObjectInfo>();

            foreach (string path in assetsPaths)
            {
                // We see if a path contains the path where our prefabs we need are located.
                if (path.Contains("Assets/Jordi/Rocks/Prefabs"))
                {
                    if (CheckObjectAtPath(path))
                    {
                        m_RockPrefabs.Add(new ObjectInfo(AssetDatabase.LoadMainAssetAtPath(path) as GameObject, path));
                    }
                }
                else if (path.Contains("Assets/MainItems/Prefabs/Folliage/FolliageEditor"))
                {
                    if (CheckObjectAtPath(path))
                    {
                        m_FolliagePrefabs.Add(new ObjectInfo(AssetDatabase.LoadMainAssetAtPath(path) as GameObject, path));
                    }
                }
            }

            // Order the objects based on their poly count
            m_RockPrefabs = m_RockPrefabs.OrderBy(o => o.Prefab.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3).ToList();
            // Order folliage based on their name
            m_FolliagePrefabs = m_FolliagePrefabs.OrderBy(o => o.Prefab.name).ToList();

            m_ObjectsLoaded = true;
        }

        private void ReloadObjects()
        {
            // Since the database is wonky at times we just get all asset paths, makes it easier to add more objects in the feature.
            string[] assetsPaths = AssetDatabase.GetAllAssetPaths();
            
            if (m_RockPrefabs == null)
            {
                m_RockPrefabs = new List<ObjectInfo>();
            }

            if (m_FolliagePrefabs == null)
            {
                m_FolliagePrefabs = new List<ObjectInfo>();
            }

            List<string> rockPaths = GetAllAddedPaths(m_RockPrefabs);
            List<string> folliagePaths = GetAllAddedPaths(m_FolliagePrefabs);  

            foreach (string path in assetsPaths)
            {
                if (rockPaths.Contains(path))
                {
                    rockPaths.Remove(path);
                    continue;
                }
                else if (folliagePaths.Contains(path))
                {
                    folliagePaths.Remove(path);
                    continue;
                }

                // We see if a path contains the path where our prefabs we need are located.
                if (path.Contains("Assets/Jordi/Rocks/Prefabs"))
                {
                    if (CheckObjectAtPath(path))
                    {
                        // Check if it's a gameobject incase non-gameobject files lands where we are looking.
                        m_RockPrefabs.Add(new ObjectInfo(AssetDatabase.LoadMainAssetAtPath(path) as GameObject, path));
                    }
                }
                else if (path.Contains("Assets/MainItems/Prefabs/Folliage/FolliageEditor/"))
                {
                    if (CheckObjectAtPath(path))
                    {
                        m_FolliagePrefabs.Add(new ObjectInfo(AssetDatabase.LoadMainAssetAtPath(path) as GameObject, path));
                    }
                }
            }

            m_RockPrefabs = RemovePathsFromPrefabList(m_RockPrefabs, rockPaths);
            m_FolliagePrefabs = RemovePathsFromPrefabList(m_FolliagePrefabs, folliagePaths);


            // Order the objects based on their poly count.
            m_RockPrefabs = m_RockPrefabs.OrderBy(o => o.Prefab.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3).ToList();
            // Order folliage based on their name
            m_FolliagePrefabs = m_FolliagePrefabs.OrderBy(o => o.Prefab.name).ToList();

            m_ObjectsRefreshing = false;
        }            

        private void SelectPrefab(ObjectInfo info)
        {
            if (info.Path.Contains("Assets/Jordi/Rocks/Prefabs"))
            {
                m_SelectedPrefabType = SelectedPrefabTypeEnum.Rock;
            }
            else if (info.Path.Contains("Assets/MainItems/Prefabs/Folliage/FolliageEditor"))
            {
                m_SelectedPrefabType = SelectedPrefabTypeEnum.Folliage;
            }

            m_SelectedPrefab = info.Prefab;
        }

        private List<string> GetAllAddedPaths(List<ObjectInfo> list)
        {
            List<string> returnList = new List<string>();

            foreach (ObjectInfo item in list)
            {
                returnList.Add(item.Path);
            }

            return returnList;
        }

        private List<ObjectInfo> RemovePathsFromPrefabList(List<ObjectInfo> prefabList, List<string> toRemovePaths)
        {
            foreach (string path in toRemovePaths)
            {   
                foreach (ObjectInfo objInfo in prefabList)
                {
                    if (objInfo.Path == path)
                    {
                        prefabList.Remove(objInfo);
                        break;
                    }
                }
            }

            return prefabList;
        }

        private void SceneGUI(SceneView sceneView)
        {
            // We get the event from the active scene here.
            Event e = Event.current;

            // On mouse up and while not holding alt we start checking to build
            if (e.type == EventType.MouseDown && !e.alt && e.button == 0)
            {
                if (m_SelectedPrefab != null)
                {
                    Tools.current = Tool.None;

                    Selection.activeGameObject = null;

                    Vector3 mousePos = e.mousePosition;
                    float pixPerPoint = EditorGUIUtility.pixelsPerPoint;
                    mousePos.y = sceneView.camera.pixelHeight - mousePos.y * pixPerPoint;
                    mousePos.x *= pixPerPoint;

                    Plane plane = new Plane(Vector3.up, GetSurface(sceneView, mousePos));
                    Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                    float enter = 0;

                    Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    if (plane.Raycast(ray, out enter))
                    {
                        Vector3 hit = ray.GetPoint(enter);
                        // Spawn in the selected prefab and set it on the hit location
                        GameObject spawnedInPrefab = PrefabUtility.InstantiatePrefab(m_SelectedPrefab) as GameObject; 
                        spawnedInPrefab.transform.position = hit;

                        switch (m_SelectedPrefabType)
                        {
                            case SelectedPrefabTypeEnum.Rock:
                                // We try to find our drawer object to store our rocks in
                                GameObject rockDrawer = GameObject.Find(m_RockDrawerName);

                                // We make one if it doesn't exist already.
                                if (rockDrawer == null)
                                {
                                    rockDrawer = new GameObject(m_RockDrawerName);
                                }

                                // And we parent our newly spawned in rock to keep the scene hierarchy clean
                                spawnedInPrefab.transform.SetParent(rockDrawer.transform);
                                break;
                            case SelectedPrefabTypeEnum.Folliage:
                                // We try to find our drawer object to store our Folliage in.
                                GameObject FolliageDrawer = GameObject.Find(m_FolliageDrawerName);

                                // We make one if it doesn't exist already.
                                if (FolliageDrawer == null)
                                {
                                    FolliageDrawer = new GameObject(m_FolliageDrawerName);
                                }

                                // And we parent our newly spawned in Folliage to keep the scene hierarchy clean.
                                spawnedInPrefab.transform.SetParent(FolliageDrawer.transform);
                                break;
                            default:
                                Debug.LogWarning("No code for drawer object for: " + m_SelectedPrefabType);
                                break;
                        }

                        // Get the current rotations
                        float xRotation = spawnedInPrefab.transform.rotation.eulerAngles.x;
                        float yRotation = spawnedInPrefab.transform.rotation.eulerAngles.y;
                        float zRotation = spawnedInPrefab.transform.rotation.eulerAngles.z;

                        // Get random rotations if set
                        if (m_RandomXRotation) xRotation = Random.Range(0, 360);
                        if (m_RandomYRotation) yRotation = Random.Range(0, 360);
                        if (m_RandomZRotation) zRotation = Random.Range(0, 360);

                        spawnedInPrefab.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

                        Selection.activeGameObject = spawnedInPrefab;

                        if (m_ClearOnPlace)
                        {
                            m_SelectedPrefab = null;
                        }

                        Undo.RegisterCreatedObjectUndo(spawnedInPrefab, "Spawned in object");

                        e.Use();
                    }
                }
            }
        }

        private Vector3 GetSurface(SceneView sceneView, Vector3 mouse)
        {
            RaycastHit hit;

            if (Physics.Raycast(sceneView.camera.ScreenPointToRay(mouse), out hit))
                return hit.point;
            return Vector3.zero;
        }

        private bool CheckObjectAtPath(string path)
        {
            // Check if it's a gameobject incase non-gameobject files lands where we are looking.
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(GameObject))
            {
                // This tool is only valid meshes so we check for mesh filter as we also call it to filter by poly count
                GameObject prefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                if (prefab.GetComponent<MeshFilter>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawList(List<ObjectInfo> list)
        {
            // Loop through our objects to display and draw them in a interactive preview GUI. 
            // Also draw a button to select the prefab.

            for (int i = 0; i < list.Count; i++)
            {
                if (i + 1 >= list.Count)
                {
                    list[i].PreviewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.whiteLabel);

                    if (GUILayout.Button("Select Object"))
                    {
                        SelectPrefab(list[i]);
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();

                    list[i].PreviewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.whiteLabel);

                    list[i + 1].PreviewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.whiteLabel);

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Select Object"))
                    {
                        SelectPrefab(list[i]);
                    }

                    if (GUILayout.Button("Select Object"))
                    {
                        SelectPrefab(list[i + 1]);
                    }

                    GUILayout.EndHorizontal();

                    i++;
                }
            }
        }

        private void IfObjectSelected()
        {
            // Object field to show the selected prefab and a button to clear selection.
            m_SelectedPrefab = EditorGUILayout.ObjectField
            ("Selected prefab.", m_SelectedPrefab, typeof(GameObject), false)
            as GameObject;

            // Toggles for random rotation per axis
            m_RandomXRotation = EditorGUILayout.Toggle("Random X rotation.", m_RandomXRotation);
            m_RandomYRotation = EditorGUILayout.Toggle("Random Y rotation.", m_RandomYRotation);
            m_RandomZRotation = EditorGUILayout.Toggle("Random Z rotation.", m_RandomZRotation);

            m_ClearOnPlace = EditorGUILayout.Toggle("Clear selection on placement", m_ClearOnPlace);

            if (GUILayout.Button("Remove selection"))
            {
                m_SelectedPrefab = null;
            }
        }
        private  void DrawerLabels()
        {
            GUILayout.Label("Drawer Names");
            // Ways to set drawer names 
            m_RockDrawerName = EditorGUILayout.TextField("Rock Drawer Name.", m_RockDrawerName);
            m_FolliageDrawerName = EditorGUILayout.TextField("Folliage Drawer Name.", m_FolliageDrawerName);
        }
       
    }
}