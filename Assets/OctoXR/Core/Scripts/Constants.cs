namespace OctoXR
{
    public static class Constants
    {
        //Layers
        public const string Hand = "Hand";
        public const string OctoPlayer = "OctoPlayer";
        public const string Grabbable = "Grabbable";
        public const string IgnoreRaycast = "Ignore Raycast";

        //Layer Numbers
        public const int OctoPlayerLayer = 29;
        public const int GrabbableLayer = 30;
        public const int HandLayer = 31;

        //Octo Settings
        public const int SolverIterations = 25;
        public const int SolverVelocityIterations = 15;
        public const float FixedTimeStep = 0.011111111f;
    }
}
