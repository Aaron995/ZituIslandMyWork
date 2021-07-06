using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class PressurePlate : MonoBehaviour, IInteractable
    {
        [Tooltip("The minimum amount of KG to activate the pressure plate.")]
        public float MinimumWeightToSolve = 25f;

        [Tooltip("The maximum amount of KG to activate the pressure plate.")]
        public float MaximumWeightToSolve = 30f;

        [Tooltip("The door the plate unlocks.")]
        public Door DoorToOpen;

        [Tooltip("The Rock snap points.")]
        public GameObject[] RockSnapPoints;

        [Tooltip("The speed the pressure plate shifts.")]
        public float ShiftSpeed = 0.5f;

        [Tooltip("The amount of resistance to being pushed down.")]
        public float PushDownResistance = 0.005f;

        [Tooltip("The moving object of the pressure plate.")]
        public GameObject MovingPart;

        [Tooltip("The amount the door moves when not solving.")]
        public float DoorMovingModifiying = 15f;        

        public InteractableType InteractType { get { return InteractableType.PressurePlate; } } // The interactable type of this object speaks for itself

        Rock[] RocksOnPlate; // The rocks on the pressure plate.
        float m_LastWeight = 0; // The weight on the last update cycle
        // Float privates used for lerping
        float m_StartY;
        float m_TargetY;
        float m_LerpTime;
        // Bool trigger for activing lerping or not
        bool m_Lerping;

        private void Awake()
        {
            if (MovingPart == null)
            {
                MovingPart = gameObject;
            }

            m_TargetY = MovingPart.transform.position.y;
            RocksOnPlate = new Rock[RockSnapPoints.Length];
        }

        private void Update()
        {
            // Get the current weight on the plate and run the update cycle if the weight has changed
            float currentWeight = WeightOnPlate();
            if (currentWeight != m_LastWeight)
            {
                // If there is enough weight on the plate fully open the door
                if (currentWeight >= MinimumWeightToSolve && currentWeight <= MaximumWeightToSolve)
                {
                    DoorToOpen.OpenDoor();
                }              

                // If the current weight is heavier means we have to move the pressure plate down
                if (currentWeight > m_LastWeight)
                {
                    float amountToMove = (currentWeight - m_LastWeight) * PushDownResistance;
                    m_StartY = m_TargetY;
                    m_TargetY = m_TargetY - amountToMove;
                    m_LerpTime = 0;
                    m_Lerping = true;

                    // Move the door as long we are under the minimum
                    if (currentWeight < MinimumWeightToSolve)
                    {
                        //Vector3 newDoorPos = DoorToOpen.TargetPosistion + new Vector3(0, (currentWeight - m_LastWeight) * DoorMovingModifiying);
                        DoorToOpen.MoveDoor();
                    }
                    // If the we are above the maximum the door slams shut 
                    else if (currentWeight > MaximumWeightToSolve && DoorToOpen.DoorState != DoorStateEnum.Closed)
                    {
                        DoorToOpen.CloseDoor();
                    }
                }
                else
                {
                    float amountToMove = (m_LastWeight - currentWeight) * PushDownResistance;
                    m_StartY = m_TargetY;
                    m_TargetY = m_TargetY + amountToMove;
                    m_LerpTime = 0;
                    m_Lerping = true;

                    // Move the door is we aren't at 0 weight and still below the minimum
                    if (currentWeight < MinimumWeightToSolve && currentWeight != 0)
                    {
                        //Vector3 newDoorPos = DoorToOpen.TargetPosistion - new Vector3(0, (m_LastWeight - currentWeight) * DoorMovingModifiying);
                        DoorToOpen.MoveDoor();
                    }
                    // If we are above the max or at 0 close the door
                    else if ((currentWeight > MaximumWeightToSolve || currentWeight == 0) && DoorToOpen.DoorState != DoorStateEnum.Closed)
                    {
                        DoorToOpen.CloseDoor();
                    }
                }
                m_LastWeight = currentWeight;
            }

            // If there is lerping to be done do it.
            if (m_Lerping)
            {
                float progress = Mathf.Lerp(m_StartY, m_TargetY, m_LerpTime / ShiftSpeed);
                m_LerpTime += Time.deltaTime;

                if (Mathf.Abs(progress - m_TargetY)  < 0.001f)
                {
                    MovingPart.transform.position = new Vector3(MovingPart.transform.position.x, m_TargetY, MovingPart.transform.position.z);
                    m_Lerping = false;
                }
                else
                {
                    MovingPart.transform.position = new Vector3(MovingPart.transform.position.x, progress, MovingPart.transform.position.z);
                }
            }
        }

        public void Interact(GameObject user, MouseClick click)
        {
            // Only react to left clicks
            if (click == MouseClick.Left)
            { 
                // Get the player inventory
                PlayerInventory playerInventory = user.GetComponent<PlayerInventory>();

                // Make sure there is an object in hand and it's a rock
                Rock rock = null;
                if (playerInventory.ObjectInHand != null)
                {
                    rock = playerInventory.ObjectInHand.GetComponent<Rock>();
                }

                if (rock != null)
                {
                    // Clear the object in the players hand and make sure we save a reference to it
                    GameObject objectPutOnPlate = playerInventory.ObjectInHand;
                    playerInventory.ClearObjectInHand();
                    // Enable the colliders again
                    Collider objectCollider = objectPutOnPlate.GetComponent<Collider>();
                    objectCollider.enabled = true;
                    // Parent it to it's snap point and put it in the right place in the array
                    objectPutOnPlate.transform.SetParent(RockSnapPoints[rock.RockIndexNumber].transform);
                    RocksOnPlate[rock.RockIndexNumber] = rock;
                    // Make sure the local pos and rotation and set to 0 
                    objectPutOnPlate.transform.localPosition = Vector3.zero;
                    objectPutOnPlate.transform.localRotation = Quaternion.identity;
                    // Subscribe to the rock pickup event
                    rock.OnPickup.AddListener(OnRockPickup);
                }
            }
        }

        void OnRockPickup(Rock rock)
        {
            rock.transform.SetParent(null);
            rock.OnPickup.RemoveListener(OnRockPickup);
            RocksOnPlate[rock.RockIndexNumber] = null;
        }

        private float WeightOnPlate()
        {
            float weight = 0;

            foreach (Rock rock in RocksOnPlate)
            {
                if (rock != null)
                {
                    weight += rock.Weight;
                }
            }

            return weight;
        }
    }
}