using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Rock : MonoBehaviour, IInteractable
    {
        [Tooltip("The weight of the rock in KG.")]
        public float Weight = 5f;

        [Tooltip("The index of the rock, used to link up with a matching snap point.")]
        public int RockIndexNumber = 0;

        [Tooltip("Destroy the rock on pickup.")]
        public bool DestroyOnPickup = false;

        [Tooltip("The extra material to give a different look to an interactable rock.")]
        public Material InteractableMaterial;

        public InteractableType InteractType { get { return InteractableType.Rock; } }

        public UnityEvent<Rock> OnPickup = new UnityEvent<Rock>();

        private void Awake()
        {
            GetComponent<Rigidbody>().mass = Weight;

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.materials = new Material[] { meshRenderer.materials[0], InteractableMaterial };
        }

        public void Interact(GameObject user, MouseClick click)
        {
            if (click == MouseClick.Left)
            {
                if (DestroyOnPickup)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    OnPickup.Invoke(this);
                    user.GetComponent<PlayerInventory>().PickupItem(gameObject);
                }
            }
        }


    }
}