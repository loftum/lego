using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bno055Sensor
{
    public class AbsArguments
    {
        public bool Temp { get; }
        public bool Gyro { get; }
        public bool LinearAccel { get; }
        public bool Velocity { get; }
        public bool Accel { get; }
        public bool Mag { get; }
        public bool Euler { get; }
        public bool Quaternion { get; }
        public bool Compass { get; }
        public bool RollPitchYaw { get; }
        public bool Calibrate { get; }

        private static readonly PropertyInfo[] Properties = typeof(AbsArguments)
            .GetProperties()
            .ToArray();

        public bool IsValid()
        {
            return Properties.Any(p => (bool)p.GetValue(this));
        }

        public static IEnumerable<string> GetPossibleArgs()
        {
            return Properties.Select(p => p.Name);
        }

        public AbsArguments(string[] args)
        {
            Quaternion = args.Has("quaternion");
            RollPitchYaw = args.Has("rpy") || args.Has("rollpitchyaw");
            Euler = args.Has("euler");
            Accel = args.Has("accel");
            Mag = args.Has("mag");
            LinearAccel = args.Has("linear");
            Velocity = args.Has("velocity");
            Gyro = args.Has("gyro");
            Temp = args.Has("temp");
            Compass = args.Has("compass");
            Calibrate = args.Has("calibrate");
        }
    }
}