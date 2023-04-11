using System.Collections.Generic;
using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// Used by the grab point controller to check for the closest grabbable and/or grab point.
    /// </summary>
    public class DistanceCheck
    {      
        public static T GetClosestObject<T>(Transform target, IEnumerable<T> objects) where T : Component
        {
            var closestDistance = float.MaxValue;
            T closestObject = null;

            foreach (var obj in objects)
            {
                var distance = (obj.transform.position - target.position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }

            return closestObject;
        }
    }
}
