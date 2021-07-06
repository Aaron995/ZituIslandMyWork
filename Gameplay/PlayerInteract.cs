using Unity.FPS.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{    
    public enum MouseClickOptions
    {
        Left,
        Right,
        Both
    }

    public struct ToolTipInfo
    {
        public bool Active;
        public string ToolTipText;
        public MouseClickOptions MouseClick;

        public ToolTipInfo(bool active = false, string toolTipText = "", MouseClickOptions mouseClick = MouseClickOptions.Both)
        {
            Active = active;
            ToolTipText = toolTipText;
            MouseClick = mouseClick;
        }
    }

    [RequireComponent(typeof(PlayerInventory), typeof(PlayerInputHandler))]
    public class PlayerInteract : MonoBehaviour
    {
        [Tooltip("The main camera for the player.")]
        public Camera PlayerCamera;

        [Tooltip("Raycast offset from the camera.")]
        public Vector3 RaycastOffset = Vector3.zero;

        [Tooltip("The maximum range the player can interact with interactables.")]
        public float MaxRangeInteract = 2.5f;

        [Tooltip("The layermask(s) the interactable objects are on.")]
        public LayerMask InteractableMayerMasks;

        PlayerInputHandler m_InputHandler;
        PlayerInventory m_PlayerInventory;
        IInteractable m_LookedAtInteractable;

        public UnityAction<ToolTipInfo> OnInteractableChange;

        void Start()
        {
            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerInteract>(m_InputHandler,
                this, gameObject);

            m_PlayerInventory = GetComponent<PlayerInventory>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInventory, PlayerInteract>(m_PlayerInventory,
                this, gameObject);
        }
      
        void Update()
        {
            LookForInteractable();
            if (m_LookedAtInteractable != null)
            {
                if (m_InputHandler.GetPlayerInteract1Input())
                {
                    m_LookedAtInteractable.Interact(gameObject, MouseClick.Left);
                }
                else if (m_InputHandler.GetPlayerInteract2Input())
                {
                    m_LookedAtInteractable.Interact(gameObject, MouseClick.Right);
                }
            }
        }

        void LookForInteractable()
        {
            RaycastHit hit;
            // Raycast from the Camera.
            if (Physics.Raycast(PlayerCamera.transform.position + RaycastOffset, PlayerCamera.transform.forward,out hit, MaxRangeInteract, InteractableMayerMasks))
            {
                // Check if the interactable has changed to save on resources constantly invoking an UnityAction.
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != m_LookedAtInteractable)
                {
                    bool returnValue;
                    if (interactable == null)
                    {
                        returnValue = false;
                        m_LookedAtInteractable = null;
                    }
                    else
                    {
                        returnValue = true;
                        m_LookedAtInteractable = interactable;
                    }

                    HandleTooltipUI(interactable,returnValue);
                }
            }
            else
            {
                if (m_LookedAtInteractable != null)
                {
                    m_LookedAtInteractable = null;
                    HandleTooltipUI(null, false);
                }

            }
        }

        void HandleTooltipUI(IInteractable interactable, bool active)
        {
            if (active && CheckIfNeedsToBeActive(interactable))
            {   
                OnInteractableChange.Invoke(new ToolTipInfo(active, GetToolTip(interactable), GetMouseClickOption(interactable)));
            }
            else
            {
                OnInteractableChange.Invoke(new ToolTipInfo());
            }
        }

        bool CheckIfNeedsToBeActive(IInteractable interactable)
        {
            // Check if we meet the requirements (if there are any) for the interactable tooltip to be active
            switch (interactable.InteractType)
            {
                case InteractableType.Rock:
                    return true;
                case InteractableType.PressurePlate:
                    if (m_PlayerInventory.ObjectInHand != null)
                    {
                        if (m_PlayerInventory.ObjectInHand.GetComponent<Rock>() != null)
                        {
                            return true;
                        }
                    }
                    return false;
                case InteractableType.LightPuzzle:
                    if (interactable.gameObject.GetComponent<WaterEndPoint>() != null)
                    {
                        return false;
                    }
                    return true;
                case InteractableType.PicturePuzzle:
                    return true;
                default:
                    Debug.LogWarning("No active state found for interactable type " + interactable.InteractType);
                    return false;
            }
        }

        string GetToolTip(IInteractable interactable)
        {
            // We get the tool tipe text pased on the interactable type
            switch (interactable.InteractType)
            {
                case InteractableType.Rock:
                    if (interactable.gameObject.GetComponent<Rock>().DestroyOnPickup)
                    {
                        return "Destroy rock";
                    }                    
                    return "Pickup rock";                    
                case InteractableType.PressurePlate:
                    return "Place rock";
                case InteractableType.LightPuzzle:           
                    return "Rotate pillar";                    
                case InteractableType.PicturePuzzle:
                    return  "Rotate Tile";                    
                default:
                    Debug.LogWarning("No tooltip text found for interactable type: " + interactable.InteractType);
                    return "Something went wrong";
            }
        }

        MouseClickOptions GetMouseClickOption(IInteractable interactable)
        {
            // We check what type the interactable is and get the mouse click data
            switch (interactable.InteractType)
            {
                case InteractableType.Rock:
                    return MouseClickOptions.Left;
                case InteractableType.PressurePlate:
                    return MouseClickOptions.Left;
                case InteractableType.LightPuzzle:
                    return MouseClickOptions.Both;
                case InteractableType.PicturePuzzle:
                    return MouseClickOptions.Left;
                default:
                    Debug.LogWarning("No MouseClickOptions found for interactable type: " + interactable.InteractType);
                    return MouseClickOptions.Both;
            }
        }
    }
}