using UnityEngine;
using UnityEngine.Events;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// Main component for adding grabbable features to a game object.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VelocityEstimator))]
    public abstract class Grabbable : MonoBehaviour, IAttachable
    {
        [Tooltip("If ticked, the grabbable object won't snap to the hand's position.")]
        [SerializeField] private bool isPrecisionGrab = true;
        public bool IsPrecisionGrab { get => isPrecisionGrab; set => isPrecisionGrab = value; }

         private InteractionHand currentInteractionHand;
        public InteractionHand CurrentInteractionHand { get => currentInteractionHand; set => currentInteractionHand = value; }

        private bool isGrabbed;
        public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }

        private bool defaultKinematicState;
        public bool DefaultKinematicState { get => defaultKinematicState; }

        private bool defaultGravityState;
        public bool DefaultGravityState { get => defaultGravityState; }

        private Rigidbody thisRigidbody;
        public Rigidbody RigidBody { get => thisRigidbody; }

        private Vector3 initialLocalScale;
        public Vector3 InitialLocalScale { get => initialLocalScale; }        

        private Transform defaultParent;
        private VelocityEstimator velocityEstimator;

        private Vector3 velocity;
        private Vector3 angularVelocity;
        private bool wasAttached;

        [Space]
        public UnityEvent OnFirstAttach;
        public UnityEvent OnAttach;
        public UnityEvent OnHeld;
        public UnityEvent OnDetach;
        public UnityEvent OnReset;

        private void Awake()
        {
            thisRigidbody = GetComponent<Rigidbody>();

            defaultGravityState = thisRigidbody.useGravity;
            defaultKinematicState = thisRigidbody.isKinematic;

            GetComponentsInChildren<GrabPoint>();            
            initialLocalScale = transform.localScale;
            velocityEstimator = GetComponent<VelocityEstimator>();
        }

        private void Update()
        {
            if (isGrabbed) OnHeld.Invoke();
        }

        /// <summary>
        /// Attaches the grabbable to the given grabParent and starts velocity estimation. Triggers OnAttach event.
        /// </summary>
        /// <param name="grabParent"></param>
        public virtual void Attach()
        {
            if (isGrabbed) return;

            StartEstimatingVelocity();

            if (!wasAttached)
            {
                OnFirstAttach.Invoke();
                OnAttach.Invoke();
                wasAttached = true;
            }
            else
            {
                OnAttach.Invoke();
            }
        }

        /// <summary>
        /// Detaches the grabbable from the object currently grabbing it and stops velocity estimation. Triggers OnDetach event.
        /// </summary>
        public virtual void Detach()
        {
            StopEstimatingVelocity();
        }

        private void StartEstimatingVelocity()
        {
            if (velocityEstimator) velocityEstimator.StartVelocityEstimation();
        }

        private void StopEstimatingVelocity()
        {   
            if (velocityEstimator != null)
            {
                VelocityEstimatorController.GetReleaseVelocities(velocityEstimator, out velocity, out angularVelocity);
                VelocityEstimatorController.SetReleaseVelocities(RigidBody, velocity, angularVelocity);
                velocityEstimator.FinishVelocityEstimation();
            }
        }
    }
}
