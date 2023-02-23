using System;
using OctoXR.Input;
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    [Serializable]
    public class ControllerInteractionInput : MonoBehaviour ,IInteractionInput
    {
        [SerializeField] private UnityXRControllerInputDataProvider inputDataProvider;
        public UnityXRControllerInputDataProvider InputDataProvider { get => inputDataProvider; }

        public bool IsProviderTracking => inputDataProvider.IsTracking;

        public bool ShouldGrab()
        {
            return inputDataProvider.Buttons.Grip.IsPressed;
        }
    }
}
