using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    [RequireComponent(typeof(PoseDataRuntime))]
    public class PosingHandSkeletonRuntimeHand : RuntimeHand
    {
        [SerializeField] private PosingHandSkeleton posingHandSkeleton;
        [SerializeField] private float rotationSpeed = 1f;

        [Tooltip("Reference to the grab controller which does the grabbing, this is automatically set on enabling the grab controller which has the reference to this component.")]
        [SerializeField] private InteractionHand interactionHand;
        public InteractionHand InteractionHand { get => interactionHand; set => interactionHand = value; }       
              
        private GrabPoint grabPoint;
        public GrabPoint GrabPoint { get => grabPoint; set => grabPoint = value; }

        protected static readonly Vector3 palmCenterOffset = new Vector3(-0.0042f, -0.0811f, -0.0189f);

        protected override void Awake()
        {
            HandType = posingHandSkeleton.HandType;

            base.Awake();
        }

        private void Start()
        {
            AddListeners(interactionHand);
        }

        private void AddListeners(InteractionHand interactionHand)
        {
            interactionHand.OnGrab.AddListener(ApplyPoseAtRuntime);
            interactionHand.OnRelease.AddListener(DisablePoseAtRuntime);
        }

        private void RemoveListeners(InteractionHand interactionHand)
        {
            interactionHand.OnGrab.RemoveListener(ApplyPoseAtRuntime);
            interactionHand.OnRelease.RemoveListener(DisablePoseAtRuntime);
        }

        public void ApplyPoseAtRuntime()
        {
            GrabPoint = interactionHand.ClosestGrabPoint;
            if (!GrabPoint) return;

            if (!GrabPoint.GrabPose) return;

            if (!GrabPoint.ParentGrabbable.IsPrecisionGrab)
            {
                posingHandSkeleton.ShouldUpdateTransforms = false;
                ApplyPose(GrabPoint.GrabPose, GrabPoint.IsPoseInverted, rotationSpeed, interactionHand);
            }
        }

        public void DisablePoseAtRuntime()
        {
            posingHandSkeleton.ShouldUpdateTransforms = true;
        }
    }
}
