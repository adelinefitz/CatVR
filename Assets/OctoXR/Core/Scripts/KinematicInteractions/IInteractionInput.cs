namespace OctoXR.KinematicInteractions
{
    interface IInteractionInput
    {
        public bool ShouldGrab();
        public bool IsProviderTracking { get; }
    }
}
