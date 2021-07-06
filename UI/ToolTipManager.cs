using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    public class ToolTipManager : MonoBehaviour
    {
        [Tooltip("The Gameobject where the tool tip parent is located.")]
        public GameObject ToolTipObject;

        [Tooltip("The text component for the tooltip.")]
        public TMPro.TextMeshProUGUI ToolTipTextComponent;

        [Tooltip("The object that holds the drop tooltip.")]
        public GameObject DropToolTipObj;

        [Header("Mouse click icons references.")]
        public GameObject LeftClickObj;
        public GameObject RightClickObj;
        public GameObject BothClickObj;

        private void Start()
        {
            PlayerInteract interact = FindObjectOfType<PlayerInteract>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerInteract, ToolTipManager>(interact, this);

            PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerInventory, ToolTipManager>(interact, this);

            interact.OnInteractableChange += InteractableToolTip;
            inventory.OnHandChange += DropToolTip;
        }

        void InteractableToolTip(ToolTipInfo info)
        {
            if (info.Active)
            {
                ToolTipObject.SetActive(true);

                switch (info.MouseClick)
                {
                    case MouseClickOptions.Left:
                        LeftClickObj.SetActive(true);
                        break;
                    case MouseClickOptions.Right:
                        RightClickObj.SetActive(true);
                        break;
                    case MouseClickOptions.Both:
                        BothClickObj.SetActive(true);
                        break;
                    default:
                        break;
                }
                ToolTipTextComponent.text = info.ToolTipText;
            }
            else
            {
                ToolTipObject.SetActive(false);
                LeftClickObj.SetActive(false);
                RightClickObj.SetActive(false);
                BothClickObj.SetActive(false);
            }
        }

        void DropToolTip(bool active)
        {
            DropToolTipObj.SetActive(active);
        }
    }
}