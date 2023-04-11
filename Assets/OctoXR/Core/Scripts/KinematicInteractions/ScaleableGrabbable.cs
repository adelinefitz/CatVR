using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// An implementation of the class which gives the grabbable object scaleable properties.
    /// </summary>
    public class ScaleableGrabbable : Grabbable
    {
        [SerializeField] private float minimumScaleFactor = 0.5f;
        [SerializeField] private float maximumScaleFactor = 1.5f;
        [SerializeField] private float scaleDistanceFactor = 2f;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float audioThreshold = 0.05f;

        private int grabberCount;

        private Transform firstPoint;
        private Transform secondPoint;

        private InteractionHand firstGrabber;
        private InteractionHand secondGrabber;

        private bool isScaling;
        private bool isRotating;
        private bool isMoving;
        private bool isFirstFrameOfRotation;

        private float grabDistance;
        private float pinchDelta;
        private float lastFrameScale;
        private float scaleStartingDistance;
        private float scaleStartingValue;
        private float currentScaleFactor = 1f;

        private Quaternion initialRotation;
        private Quaternion previousFirstPointRotation;
        private Quaternion previousSecondPointRotation;
        private Quaternion targetRotation;
        private Quaternion activeRotation;
        private Quaternion finalRotation;

        private Rigidbody baseRigidbody;

        private GameObject positionAnchor;

        private void Start()
        {
            IsPrecisionGrab = true;
            baseRigidbody = gameObject.GetComponent<Rigidbody>();
            SetPositionAnchor();
        }

        private void SetPositionAnchor()
        {
            if (positionAnchor) return;
            positionAnchor = new GameObject();
            positionAnchor.transform.SetParent(transform);
            positionAnchor.name = "PositionAnchor";
        }

        public override void Attach()
        {
            base.Attach();

            HandleAttaching(CurrentInteractionHand.transform);
        }

        private void HandleAttaching(Transform grabParent)
        {
            grabberCount++;

            if (grabberCount > 0 && grabberCount < 2)
            {
                FirstAttach(grabParent);
            }

            if (grabberCount >= 2)
            {
                SecondAttach(grabParent);
            }
        }

        private void FirstAttach(Transform grabParent)
        {
            transform.SetParent(grabParent);
            grabParent.localRotation = Quaternion.Euler(0, 0, 0);
            firstGrabber = grabParent.GetComponent<InteractionHand>();
            firstPoint = grabParent;
        }

        private void SecondAttach(Transform grabParent)
        {
            secondGrabber = grabParent.GetComponent<InteractionHand>();
            secondPoint = grabParent;

            grabDistance = (firstPoint.position - secondPoint.position).magnitude;

            transform.SetParent(null);

            // 
            // movement start method
          
                isMoving = true;

            // rotation start method
            if (isRotating) return;
            Vector3 diff = secondPoint.position - firstPoint.position;
            activeRotation = Quaternion.LookRotation(diff, Vector3.up).normalized;
            isRotating = true;
            isFirstFrameOfRotation = true;

            // scale start method
            isScaling = true;
            scaleStartingDistance = grabDistance;
            scaleStartingValue = currentScaleFactor;
            lastFrameScale = transform.localScale.magnitude;
        }

        private void Update()
        {
            if (isScaling)
            {
                grabDistance = (firstPoint.position - secondPoint.position).magnitude;
                pinchDelta = grabDistance - scaleStartingDistance;
                // scale update
                var newScaleFactor = scaleStartingValue + (pinchDelta * scaleDistanceFactor);

                if (newScaleFactor > minimumScaleFactor && newScaleFactor < maximumScaleFactor)
                {
                    currentScaleFactor = newScaleFactor;                    
                    transform.localScale = Vector3.Lerp(transform.localScale, InitialLocalScale * newScaleFactor, 10);
                }

                PlayAudio();
            }

            if (isRotating)
            {
                // rotation update

                var worldAxisCurrent = secondPoint.position - firstPoint.position;

                if (!isFirstFrameOfRotation)
                {
                    initialRotation = activeRotation;
                    var targetTransformRotation = transform.rotation;
                    baseRigidbody.isKinematic = false;

                    baseRigidbody.velocity = Vector3.zero;
                    baseRigidbody.angularVelocity = Vector3.zero;

                    var previousUpAxis = initialRotation * Vector3.up;
                    var newUpAxis = worldAxisCurrent.magnitude * previousUpAxis;

                    var point1Angle = GetRotationDegrees(newUpAxis, previousFirstPointRotation, firstPoint.rotation, worldAxisCurrent);
                    var point1AngleHalf = point1Angle / 2;

                    var point2Angle = GetRotationDegrees(newUpAxis, previousSecondPointRotation, secondPoint.rotation, worldAxisCurrent);
                    var point2AngleHalf = point2Angle / 2;

                    var avgAngle = point1AngleHalf + point2AngleHalf;
                    newUpAxis = Quaternion.AngleAxis(avgAngle, worldAxisCurrent) * previousUpAxis;

                    targetRotation = Quaternion.LookRotation(worldAxisCurrent, newUpAxis);
                    activeRotation = targetRotation;

                    var rotationInTargetSpace = Quaternion.Inverse(initialRotation) * targetTransformRotation;
                    finalRotation = targetRotation * rotationInTargetSpace;

                    transform.rotation = finalRotation;
                }
                else
                {
                    isFirstFrameOfRotation = false;
                }

                previousFirstPointRotation = firstPoint.rotation;
                previousSecondPointRotation = secondPoint.rotation;
            }

            if (isMoving)
            {
                // movement update

                positionAnchor.transform.SetParent(null);
                var middlePoint = ((secondPoint.position + firstPoint.position)) / 2;
                positionAnchor.transform.position = middlePoint;
                transform.SetParent(positionAnchor.transform);
            }
        }

        private float GetRotationDegrees(Vector3 originalUpVector, Quaternion previousPointRotation, Quaternion currentPointRotation, Vector3 axis)
        {
            Vector3 newUpVector = (currentPointRotation * Quaternion.Inverse(previousPointRotation)) * originalUpVector;
            newUpVector = Vector3.ProjectOnPlane(newUpVector, axis);

            return Vector3.SignedAngle(originalUpVector, newUpVector, axis);
        }

        public override void Detach()
        {
            base.Detach();

            HandleDetach();
        }

        private void HandleDetach()
        {
            grabberCount--;

            if (grabberCount > 0 && grabberCount <= 2)
            {
                FirstDetach();
            }

            if (grabberCount == 0)
            {
                SecondDetach();
            }

            RigidBody.isKinematic = true;

            //position stop

            isMoving = false;
            positionAnchor.transform.SetParent(transform);
            positionAnchor.transform.localPosition = Vector3.zero;

            //scale stop

            isScaling = false;
            scaleStartingValue = 0;
            lastFrameScale = transform.localScale.magnitude;

            //rotation stop

            isRotating = false;
            isFirstFrameOfRotation = false;
        }

        private void FirstDetach()
        {
            if (!firstGrabber.IsGrabbing)
            {
                firstGrabber = secondGrabber;
                secondGrabber = null;
                firstPoint = firstGrabber.transform;
                secondPoint = null;
            }

            transform.SetParent(firstPoint);
        }

        private void SecondDetach()
        {
            transform.SetParent(null);
            IsGrabbed = false;

            firstGrabber = null;
            secondGrabber = null;
            firstPoint = null;
            secondPoint = null;
        }

        public void PlayAudio()
        {
            if (!audioSource) return;

            var currentFrameScale = transform.localScale.magnitude;
            var velocity = (currentFrameScale - lastFrameScale) * 5;

            if ((velocity > audioThreshold || velocity < -audioThreshold) && !audioSource.isPlaying)
            {
                audioSource.volume = 1 * Mathf.Clamp(Mathf.Abs(velocity), 0.3f, 1f);
                audioSource.Play();
            }

            lastFrameScale = currentFrameScale;
        }
    }
}