
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// An implementation of the InteractionModifier class, used to look for objects in the close proximity of the interactor.
    /// </summary>
    public class ProximityInteractionHand : InteractionHand
    {
        private Collider firstColliderHit;
        [SerializeField] private float overlapSphereRadius = 0.01f;

        public override void StartInteraction()
        {
            base.StartInteraction();

            var check = ContactCheck();

            if (check.Length > 0)
            {
                firstColliderHit = check[0];
                CheckForGrabbable(firstColliderHit);
                UpdateInteraction();
            }
        }

        public override void EndInteraction()
        {
            base.EndInteraction();
            CheckForRelease();
        }

        private void CheckForGrabbable(Collider foundCollider)
        {
            if (GrabbedObject) return;
            if (!foundCollider.attachedRigidbody) return;

            AddGrabbableObject(foundCollider);
            AddGrabPoint(foundCollider);
        }

        private void CheckForRelease()
        {
            if (!firstColliderHit) return;
            if (!firstColliderHit.attachedRigidbody) return;

            var grabbable = firstColliderHit.attachedRigidbody.GetComponent<Grabbable>();
            if (!grabbable) return;

            var grabPoints = grabbable.GetComponentsInChildren<GrabPoint>();

            if (grabbable && ObjectsInReach.Contains(grabbable))
            {
                RemoveGrabbableObject(grabbable);
                RemoveGrabPoint(grabPoints);
            }

            if (ObjectsInReach.Count == 0 && !grabbable.IsGrabbed)
            {
                ClosestObject = null;
                ClosestGrabPoint = null;
            }
        }

        public virtual Collider[] ContactCheck()
        {
            var contactResult = UnityEngine.Physics.OverlapSphere(transform.position, overlapSphereRadius, LayerMask);
            return contactResult;
        }
    }
}
