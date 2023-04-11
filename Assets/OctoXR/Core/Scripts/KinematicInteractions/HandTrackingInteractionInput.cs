using System;
using OctoXR.Collections;
using OctoXR.Input;
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    [Serializable]
    public class HandTrackingInteractionInput : MonoBehaviour, IInteractionInput
    {
        [SerializeField] private HandInputDataProvider inputDataProvider;
        public HandInputDataProvider InputDataProvider { get => inputDataProvider; }

        [Range(0.09f, 0.12f)][SerializeField] private float grabThreshold = 0.1f;

        private HandBoneKeyedReadOnlyCollection<Pose> bonePoses;
        public bool IsProviderTracking => inputDataProvider.IsTracking;

        public bool ShouldGrab()
        {
            bonePoses = inputDataProvider.GetBoneAbsolutePoses();
            var wrist = bonePoses[0];
            var distanceBetweenFingersAndWrist = 0.0f;

            foreach (var bonePose in bonePoses)
            {
                distanceBetweenFingersAndWrist += Mathf.Abs((wrist.position - bonePose.position).magnitude);
            }

            var averageDistance = distanceBetweenFingersAndWrist / bonePoses.Count;
            return averageDistance <= grabThreshold;
        }
    }
}
