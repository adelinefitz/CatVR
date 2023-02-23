namespace OctoXR.KinematicInteractions
{
    /// <summary>
    /// Interface used to give an object Interaction start, update and end methods. Primarily used by GrabController.
    /// </summary>
    public interface IInteractor
    {
        public void StartInteraction();
        public void UpdateInteraction();
        public void EndInteraction();
    }
}
