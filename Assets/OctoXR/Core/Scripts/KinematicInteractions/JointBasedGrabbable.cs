using UnityEngine;
using UnityEngine.Events;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// An implementation of the class which gives the grabbable joint based properties.
    /// </summary>
    public class JointBasedGrabbable : Grabbable
    {
        [Header("Joint values")]
        [SerializeField]
        private float spring = 3000;

        [SerializeField] private float angularSpring = 1000;
        [SerializeField] private float damper = 50;
        [SerializeField] private float maximumAllowedForce = 10000;
        [SerializeField] private float breakForce = 5000;
        [SerializeField] private float allowedMaximumDistance;

        private ConfigurableJoint configurableJoint;

        public override void Attach()
        {
            base.Attach();

            RigidBody.isKinematic = true;
            RigidBody.useGravity = DefaultGravityState;

            CreateJoint();
        }

        private void CreateJoint()
        {
            if (CurrentInteractionHand == null || gameObject.GetComponent<Rigidbody>() == null)
            {
                Debug.LogError("Cannot create joint: CurrentInteractionHand or Rigidbody is null");
                return;
            }

            DestroyJoint();

            var rb = gameObject.GetComponent<Rigidbody>();

            transform.parent = CurrentInteractionHand.TrackingSpace;

            configurableJoint = CurrentInteractionHand.HandSkeleton.gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.connectedBody = rb;

            configurableJoint.autoConfigureConnectedAnchor = true;
            configurableJoint.enablePreprocessing = true;

            configurableJoint.connectedAnchor = transform.InverseTransformPoint(CurrentInteractionHand.transform.position);

            configurableJoint.xMotion = configurableJoint.yMotion = configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = configurableJoint.angularYMotion = configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

            SetMotionDrive();
            SetAngularDrive();

            SetBreakForces(breakForce);

            rb.isKinematic = false;
            rb.useGravity = false;

            IsGrabbed = true;
        }

        private void SetBreakForces(float breakForce)
        {
            configurableJoint.breakForce = breakForce;
            configurableJoint.breakTorque = breakForce;
        }

        private void SetMotionDrive()
        {
            var motionDrive = new JointDrive
            {
                positionSpring = spring,
                positionDamper = damper,
                maximumForce = maximumAllowedForce
            };
            configurableJoint.xDrive = configurableJoint.yDrive = configurableJoint.zDrive = motionDrive;
        }

        private void SetAngularDrive()
        {
            var angularDrive = new JointDrive
            {
                positionSpring = angularSpring,
                positionDamper = damper,
                maximumForce = maximumAllowedForce
            };
            configurableJoint.angularXDrive = configurableJoint.angularYZDrive = angularDrive;
        }

        private void DestroyJoint()
        {
            if (configurableJoint != null)
            {
                Destroy(configurableJoint);
                configurableJoint = null;
            }
        }

        public override void Detach()
        {
            base.Detach();

            DestroyJoint();
            if (RigidBody != null)
            {
                RigidBody.isKinematic = DefaultKinematicState;
                RigidBody.useGravity = DefaultGravityState;
            }

            IsGrabbed = false;
            RigidBody.isKinematic = DefaultKinematicState;
            RigidBody.useGravity = DefaultGravityState;

            transform.parent = null;

            if (transform != null)
            {
                transform.parent = null;
            }
            CurrentInteractionHand = null;
            OnDetach?.Invoke();
        }
    }
}
