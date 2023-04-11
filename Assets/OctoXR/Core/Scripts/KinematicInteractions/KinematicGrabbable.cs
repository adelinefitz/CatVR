using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// An implementation of the class which gives the grabbable object kinematic properties.
    /// </summary>
    public class KinematicGrabbable : Grabbable
    {
        public override void Attach()
        {
            base.Attach();

            transform.SetParent(CurrentInteractionHand.transform);

            RigidBody.isKinematic = true;
            RigidBody.useGravity = false;

            IsGrabbed = true;
        }

        public override void Detach()
        {
            base.Detach();
            IsGrabbed = false;

            transform.parent = null;

            RigidBody.isKinematic = DefaultKinematicState;
            RigidBody.useGravity = DefaultGravityState;

            CurrentInteractionHand = null;

            OnDetach.Invoke();
        }
    }
}
