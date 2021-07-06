using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public enum InteractableType
    {
        Rock,
        PressurePlate,
        LightPuzzle,
        PicturePuzzle
    }

    public enum MouseClick
    {
        Left,
        Right
    }

    public interface IInteractable 
    {
        public GameObject gameObject { get; }

        public InteractableType InteractType { get; }

        /// <summary>
        /// Way to interact with the object.
        /// </summary>
        /// <param name="user">The user interacting with the object.</param>
        /// <param name="mouseClick">The mouse button clicked.</param>
        public void Interact(GameObject user, MouseClick mouseClick);    
    }

}