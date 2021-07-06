using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class PlayerInventory : MonoBehaviour
    {
        [Tooltip("The default posistion the object held by the player will be hold.")]
        public Transform DefaultObjectHoldPosistion;

        public GameObject ObjectInHand { private set; get; }

        public UnityAction<bool> OnHandChange;

        PlayerInputHandler m_InputHandler;

        private void Awake()
        {
            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerInteract>(m_InputHandler,
                this, gameObject);
        }

        private void Update()
        {
            if (m_InputHandler.GetPlayerInteract2Input())
            {
                DropItem();
            }
        }

        /// <summary>
        /// To pick up an item and hold it in the player's hand.
        /// </summary>
        /// <param name="item">The item being held.</param>
        public void PickupItem(GameObject item)
        {
            // If we have an object in hand we drop it
            if (ObjectInHand != null)
            {
                DropItem();
            }
            
            ObjectInHand = item;

            // Make the object being held kinematic and disable to collider
            Rigidbody itemRB = item.GetComponent<Rigidbody>();
            if (itemRB != null)
                itemRB.isKinematic = true;

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null)
                itemCollider.enabled = false;

            // Set the item as a child to our hold posistion and set the local transform to zero
            item.transform.SetParent(DefaultObjectHoldPosistion);
            item.transform.localPosition = Vector3.zero;

            OnHandChange.Invoke(true);
        }

        /// <summary>
        /// Drops the item we currently have in our hand.
        /// </summary>
        public void DropItem()
        {
            if (ObjectInHand != null)
            {
                ObjectInHand.transform.position = ObjectInHand.transform.position + (transform.forward * 1.5f);

                Rigidbody itemRB = ObjectInHand.GetComponent<Rigidbody>();
                if (itemRB != null)
                    itemRB.isKinematic = false;

                Collider itemCollider = ObjectInHand.GetComponent<Collider>();
                if (itemCollider != null)
                    itemCollider.enabled = true;

                ObjectInHand.transform.SetParent(null);
                ObjectInHand = null;

                OnHandChange.Invoke(false);
            }
        }

        /// <summary>
        /// Clears the object we have in our hand. THIS DOES NOT DROP THE ITEM.
        /// </summary>
        public void ClearObjectInHand()
        {
            if (ObjectInHand != null)
            {
                ObjectInHand.transform.SetParent(null);

                ObjectInHand = null;

                OnHandChange.Invoke(false);
            }
        }
    }
}
