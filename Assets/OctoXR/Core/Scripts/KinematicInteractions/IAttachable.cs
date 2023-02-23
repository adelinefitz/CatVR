using UnityEngine;

namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// Interface used to give an object Attach and Detach methods - primarily used by grabbables.
    /// </summary>
    public interface IAttachable
    {
        public void Attach();

        public void Detach();
    }
}