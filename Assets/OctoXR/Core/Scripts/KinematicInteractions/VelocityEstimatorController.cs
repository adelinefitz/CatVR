using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    public class VelocityEstimatorController
    {
        public static void GetReleaseVelocities(VelocityEstimator velocityEstimator, out Vector3 velocity, out Vector3 angularVelocity)
        {
            velocity = velocityEstimator.GetLinearVelocityAverage();
            angularVelocity = velocityEstimator.GetAngularVelocityAverage();
        }

        public static void SetReleaseVelocities(Rigidbody rigidBody, Vector3 velocity, Vector3 angularVelocity)
        {
            rigidBody.velocity = velocity;
            rigidBody.angularVelocity = angularVelocity;
        }
    }
}