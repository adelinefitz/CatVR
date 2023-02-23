using System;
using System.Collections;
using System.Collections.Generic;
using OctoXR.UI;
using UnityEngine;
using UnityEngine.Events;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// Script that detects which input method is currently used (hand tracking or controllers), and switches between them in runtime.
    /// </summary>
    public abstract class InteractionHand : MonoBehaviour, IInteractor
    {
        [Tooltip("Layer that you want the interactor to interact with.")]
        [SerializeField] private LayerMask layerMask;

        public LayerMask LayerMask { get => layerMask; }
        private List<Grabbable> objectsInReach = new List<Grabbable>();
        public List<Grabbable> ObjectsInReach { get => objectsInReach; }

        private List<GrabPoint> grabPointsInReach = new List<GrabPoint>();
        public List<GrabPoint> GrabPointsInReach { get => grabPointsInReach; }

        private GrabPoint closestGrabPoint;
        public GrabPoint ClosestGrabPoint { get => closestGrabPoint; set => closestGrabPoint = value; }

        private Grabbable grabbedObject;
        public Grabbable GrabbedObject { get => grabbedObject; set => grabbedObject = value; }

        private Grabbable closestObject;
        public Grabbable ClosestObject { get => closestObject; set => closestObject = value; }

        private Grabbable lastClosestObject;
        public Grabbable LastClosestObject { get => lastClosestObject; set => lastClosestObject = value; }

        [SerializeField] private Transform palmCenter;
        public Transform PalmCenter { get => palmCenter; }

        [SerializeField] private Transform trackingSpace;
        public Transform TrackingSpace { get => trackingSpace; }

        [SerializeField] private HandSkeleton handSkeleton;
        public HandSkeleton HandSkeleton { get => handSkeleton; }

        private bool shouldGrab;
        public bool ShouldGrab { get => shouldGrab; }

        private bool isGrabbing;
        public bool IsGrabbing { get => isGrabbing; }

        [SerializeField] private float smoothingValue = 0.1f;

        private HandType handSkeletonType;
        private bool hasObject;
        private bool positionReached;
        private Coroutine activeCoroutine;
        private Vector3 velocity = Vector3.zero;

        private IInteractionInput[] interactionInputs;
        private IInteractionInput currentInteractionInput;

        public UnityEvent OnGrab;
        public UnityEvent OnRelease;
        public UnityEvent OnHandTrackingActivated;
        public UnityEvent OnControllersActivated;

        private void Awake()
        {
            handSkeletonType = handSkeleton.HandType;
            interactionInputs = GetComponentsInChildren<IInteractionInput>();

            transform.SetParent(palmCenter);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void Start()
        {
            if (interactionInputs.Length <= 0)
            {
                Debug.LogWarning("No interaction input detected!");
                return;
            }

            SwitchInput();
        }

        private void SwitchInput()
        {
            foreach (var input in interactionInputs)
            {
                if (input.IsProviderTracking)
                {
                    currentInteractionInput = input;
                    break;
                }
            }
        }

        private void Update()
        {
            if (interactionInputs.Length <= 0)
            {
                Debug.LogWarning("No interaction input detected!");
                return;
            }

            SwitchInput();
            DetectGrab();
            ProcessGrab();
        }

        public virtual void ProcessGrab()
        {
            GrabCheck();
        }

        public void DetectGrab()
        {
            if(currentInteractionInput != null) shouldGrab = currentInteractionInput.ShouldGrab();
        }
        public virtual void GrabCheck()
        {
            if (!isGrabbing && shouldGrab) StartInteraction();

            if (!shouldGrab && isGrabbing) EndInteraction();
        }

        public virtual void StartInteraction()
        {
            isGrabbing = true;
        }

        public virtual void UpdateInteraction()
        {
            if (!closestObject) return;
            if (closestObject.IsGrabbed) return;

            if (!hasObject && !closestObject.IsPrecisionGrab)
            {
                closestObject.RigidBody.isKinematic = true;
                closestObject.RigidBody.useGravity = false;

                AdjustGrabbablePosition(closestObject, closestGrabPoint);
                StartCoroutine(WaitUntilPositionReached());
            }
            else if (!hasObject && closestObject.IsPrecisionGrab)
            {
                closestObject.CurrentInteractionHand = this;
                closestObject.Attach();
            }

            grabbedObject = closestObject;
            OnGrab.Invoke();
        }

        public virtual void EndInteraction()
        {
            if (activeCoroutine != null) KillCoroutine();

            if (!isGrabbing) return;
            isGrabbing = false;

            if (!grabbedObject) return;
            OnRelease.Invoke();
            GrabbedObject.Detach();

            positionReached = false;

            grabbedObject = null;

            hasObject = false;
            closestObject = DistanceCheck.GetClosestObject(transform, objectsInReach);
            
        }

        public IEnumerator WaitUntilPositionReached()
        {
            yield return new WaitUntil(() => positionReached);

            closestObject.CurrentInteractionHand = this;
            closestObject.Attach();

            hasObject = true;
        }

        private void AdjustGrabbablePosition(Grabbable connectedObject, GrabPoint grabPoint)
        {
            if (activeCoroutine != null)
            {
                KillCoroutine();
            }

            activeCoroutine = StartCoroutine(GrabbablePositionCoroutine(connectedObject, grabPoint));
        }

        private IEnumerator GrabbablePositionCoroutine(Grabbable connectedObject, GrabPoint grabPoint)
        {
            if (positionReached) yield return null;

            connectedObject.transform.SetParent(transform, true);

            Vector3 difference = connectedObject.transform.position - (transform.position - connectedObject.transform.rotation * grabPoint.Offset);

            while (difference.sqrMagnitude > 0.00001)
            {
                connectedObject.transform.rotation = Quaternion.Lerp(connectedObject.transform.rotation,
                   transform.rotation * Quaternion.Inverse(grabPoint.RotationOffset), smoothingValue);

                connectedObject.transform.position = Vector3.SmoothDamp(connectedObject.transform.position,
                    transform.position - connectedObject.transform.rotation * grabPoint.Offset, ref velocity, smoothingValue);

                difference = connectedObject.transform.position - (transform.position - connectedObject.transform.rotation * grabPoint.Offset);
                yield return null;
            }

            connectedObject.transform.position = transform.position - grabbedObject.transform.rotation * closestGrabPoint.Offset;
            connectedObject.transform.rotation = transform.rotation * Quaternion.Inverse(closestGrabPoint.RotationOffset);

            positionReached = true;

            yield return null;
        }

        /// <summary>
        /// Adds a grab point to an array of grab points within reach detected by the grab controller.
        /// </summary>
        /// <param name="grabPointCollider"></param>
        public void AddGrabPoint(Collider grabPointCollider)
        {
            if (grabPointCollider == null) return;

            var grabPoints = grabPointCollider.GetComponentsInChildren<GrabPoint>();

            foreach (var grabPoint in grabPoints)
            {
                if (closestObject != grabPoint.ParentGrabbable) continue;
                if (grabPointsInReach.Contains(grabPoint)) continue;

                if (grabPoint.IsGrabbed) continue;

                if (handSkeletonType == grabPoint.HandType)
                {
                    grabPointsInReach.Add(grabPoint);
                }
            }

            if (closestGrabPoint == null)
            {
                closestGrabPoint = DistanceCheck.GetClosestObject(this.transform, grabPointsInReach);
            }
        }

        /// <summary> 
        /// Adds a grabbable to an array of grabbables within reach detected by the grab controller.
        /// </summary>
        /// <param name="grabbableCollider"></param>
        public void AddGrabbableObject(Collider grabbableCollider)
        {
            if (grabbableCollider == null) return;

            var objectInReach = grabbableCollider.GetComponentInChildren<Grabbable>();

            if (objectInReach)
            {
                if (objectInReach.IsGrabbed) return;
                if (!objectsInReach.Contains(objectInReach)) objectsInReach.Add(objectInReach);
            }

            if (closestObject == null)
            {
                closestObject = DistanceCheck.GetClosestObject(this.transform, objectsInReach);
            }
        }

        public void RemoveGrabPoint(GrabPoint[] grabPoints)
        {
            if (grabPoints == null) return;
            for (int i = 0; i < grabPoints.Length; i++)
            {
                if (grabPointsInReach.Contains(grabPoints[i]))
                    grabPointsInReach.Remove(grabPoints[i]);
            }
        }

        public void RemoveGrabbableObject(Grabbable grabbable)
        {
            if (grabbable == null) return;
            if (objectsInReach.Contains(grabbable))
                objectsInReach.Remove(grabbable);
        }

        public void KillCoroutine()
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

    }
}
