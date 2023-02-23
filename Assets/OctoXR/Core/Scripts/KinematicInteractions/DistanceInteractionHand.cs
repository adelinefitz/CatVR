using OctoXR.KinematicInteractions.Utilities;
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// An implementation of the InteractionModifier class, used to look for objects at a set distance from the interactor.
    /// </summary>
    public class DistanceInteractionHand : InteractionHand
    {
        [Tooltip("Position from which the actual raycast should be started.")]
        [SerializeField] private Transform raycastSource;
        [Tooltip("Position from which the representation ray should be drawn.")]
        [SerializeField] private Transform rayVisualSource;
        
        [SerializeField][Range(0f, .1f)] private float radius = 0.01f;
        [Tooltip("Line renderer you want to use to draw the ray visuals.")]
        [SerializeField] private LineRenderer lineRenderer;
        [Tooltip("Maximum distance of the raycast.")]
        [SerializeField][Range(0f, 100f)] private float maxRayDistance = 30f;
        [Tooltip("If ticked, the grabbable is detected via a spherecast - this can be easier to find grabbables.")]
        [SerializeField] private bool isSphereCast = true;
        [Tooltip("Should the line renderer be drawn or will the interaction be invisible.")]
        [SerializeField] private bool shouldDrawLineRenderer;

        private Grabbable hitObject;
        private RaycastHit hitInfo;
        private Vector3 raycastSourcePosition;
        private bool hasHit;

        public override void StartInteraction()
        {
            base.StartInteraction();
            UpdateInteraction();
        }

        public override void ProcessGrab()
        {
            FindClosestGrabbable();
            if (shouldDrawLineRenderer) DrawLineRenderer();
            LastClosestObject = ClosestObject;
            GrabCheck();
        }

        private void DrawLineRenderer()
        {
            if (ClosestObject && !ClosestObject.IsGrabbed)
            {
                lineRenderer.enabled = true;
                RaycastVisuals.DrawLine(rayVisualSource.position, ClosestObject.transform.position, lineRenderer);
            }
            else if (GrabbedObject)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }

        private void FindClosestGrabbable()
        {
            if (GrabbedObject) return;
            raycastSourcePosition = raycastSource.position;

            if (isSphereCast)
                hasHit = UnityEngine.Physics.SphereCast(raycastSourcePosition, radius, raycastSource.forward, out hitInfo, maxRayDistance, LayerMask);
            else
                hasHit = UnityEngine.Physics.Raycast(raycastSourcePosition, raycastSource.forward, out hitInfo, maxRayDistance, LayerMask);

            if (!hasHit)
            {
                ClosestObject = null;
                GrabPointsInReach.Clear();
                return;
            }

            var hitDistance = Vector3.Distance(hitInfo.point, raycastSourcePosition);
            if (hitDistance > maxRayDistance)
            {
                ClosestObject = null;
                GrabPointsInReach.Clear();
                return;
            }

            hitObject = hitInfo.collider.gameObject.GetComponent<Grabbable>();

            ClosestObject = hitObject;

            AddGrabbableObject(hitInfo.collider);
            AddGrabPoint(hitInfo.collider);

            ClosestGrabPoint = DistanceCheck.GetClosestObject(transform, GrabPointsInReach);
        }
    }
}
