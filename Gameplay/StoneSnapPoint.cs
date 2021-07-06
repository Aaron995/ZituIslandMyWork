using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Previews a rock during editor time, only used for a preview can be removed from the snappoints.
    /// </summary>
    public class StoneSnapPoint : MonoBehaviour
    {
        [Tooltip("The index of the snap point, used to link up with a matching rock.")]
        public int Index = 0;
        [Tooltip("Enable to preview rock.")]
        public bool EnablePreview;

        [SerializeField] GameObject m_LinkedRock;
        
        void Awake()
        {
            // If there is a linked rock remove it
            if (m_LinkedRock != null)
            {
                Destroy(m_LinkedRock);
            }
        }


#if(UNITY_EDITOR)
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && EnablePreview)
            {
                if (m_LinkedRock == null)
                {
                    Rock[] rocks = FindObjectsOfType<Rock>();

                    foreach (Rock rock in rocks)
                    {
                        if (rock.RockIndexNumber == Index)
                        {
                            m_LinkedRock = Instantiate(rock.gameObject);
                            m_LinkedRock.transform.SetParent(transform);
                            m_LinkedRock.transform.localPosition = Vector3.zero;
                            m_LinkedRock.transform.localRotation = Quaternion.identity;
                            m_LinkedRock.GetComponent<Rock>().RockIndexNumber = -9999;
                        }
                    }
                }
                else
                {
                    m_LinkedRock.transform.localRotation = Quaternion.identity;
                    m_LinkedRock.transform.localPosition = Vector3.zero;
                }

                if (transform.childCount > 1)
                {
                    List<GameObject> toRemoveChildern = new List<GameObject>();
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).GetComponent<Rock>().RockIndexNumber != Index)
                        {
                            toRemoveChildern.Add(transform.GetChild(i).gameObject);
                        }
                    }

                    foreach (GameObject toRemoveChild in toRemoveChildern)
                    {
                        DestroyImmediate(toRemoveChild);
                    }
                }
            }
            else if (!Application.isPlaying && !EnablePreview)
            {
                if (m_LinkedRock != null)
                {
                    DestroyImmediate(m_LinkedRock);
                }
            }
        }
#endif
    }
}