using CoreMotion;

namespace SteeringWheel
{
    public static class MotionManager
    {
        public static CMMotionManager Instance { get; }

        static MotionManager()
        {
            Instance = new CMMotionManager();
        }
    }
}

