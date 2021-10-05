using System;
using System.Collections.Generic;
using Maths;

namespace Lego.Core
{
    public class LegoCarState
    {
        public Double3 EulerAngles { get; set; }
        public Quatd Quaternion { get; set; }
        public Int2 Throttle { get; set; }
        public Int2 Motion { get; set; }
        
        public List<double> Distances { get; set; } = new List<double>();

        public override string ToString() => Serialize();

        public string Serialize()
        {
            return $"{EulerAngles};{Quaternion};{Throttle};{Motion};[{string.Join(",", Distances)}]";
        }

        public static bool TryParse(string serialized, out LegoCarState state)
        {
            state = null;
            var parts = serialized.Split(';');
            if (parts.Length != 5)
            {
                Console.WriteLine("Bad length");
                return false;
            }

            if (!Double3.TryParse(parts[0], out var euler))
            {
                Console.WriteLine("Bad euler");
                return false;
            }
            if (!Quatd.TryParse(parts[1], out var quaternion))
            {
                Console.WriteLine("Bad quaternion");
                return false;
            }

            if (!Int2.TryParse(parts[2], out var throttle))
            {
                Console.WriteLine("Bad speed");
                return false;
            }

            if (!Int2.TryParse(parts[3], out var motion))
            {
                Console.WriteLine("Bad motion");
                return false;
            }

            if (!Doubles.TryParse(parts[4], out var distances))
            {
                Console.WriteLine("Bad doubles");
                return false;
            }

            state = new LegoCarState
            {
                EulerAngles = euler,
                Quaternion = quaternion,
                Throttle = throttle,
                Motion = motion,
                Distances = distances
            };
            return true;
        }
    }
}