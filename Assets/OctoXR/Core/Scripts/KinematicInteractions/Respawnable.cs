using System.Collections;
using OctoXR.KinematicInteractions.Utilities;
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    public class Respawnable : MonoBehaviour
    {
        [SerializeField] private float respawnTime;

        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        private Grabbable grabbable;
        private Rigidbody rb;

        private bool hasCollided;

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            rb = GetComponent<Rigidbody>();
        }

        public void SetDefaultTransformValues()
        {
            transform.position = defaultPosition;
            transform.rotation = defaultRotation;
        }

        public void SetDefaultRigidbodyValues(Rigidbody rigidbody, Transform transform)
        {
            rigidbody.isKinematic = grabbable.DefaultKinematicState;
            rigidbody.useGravity = grabbable.DefaultGravityState;
        }

        public void SetNewRigidbodyValues(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var reset = collision.gameObject.GetComponent<ResetObject>();
            if (!reset) return;

            if (reset && !hasCollided)
            {
                hasCollided = true;
                var respawnObject = RespawnObject(reset.RespawnTime, reset.PrefabSpawner, reset.Particle);
                StartCoroutine(respawnObject);
            }
        }

        public IEnumerator RespawnObject(float respawnTime, PrefabSpawner prefabSpawner, GameObject particle)
        {
            yield return new WaitForSeconds(respawnTime);
            if (grabbable.IsGrabbed)
            {
                hasCollided = false;
                yield break;
            }

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            SetDefaultTransformValues();
            SetDefaultRigidbodyValues(rb, transform);

            prefabSpawner.SpawnPrefabAtTransform(particle, transform, null, respawnTime);
            hasCollided = false;
            grabbable.OnReset.Invoke();
        }
    }
}
