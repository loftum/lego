using System;
using System.Collections.Generic;
using Maths;

namespace Lego.Core
{
    public class LegoCarState
    {
        public Double3 EulerAngles { get; set; }
        public Quatd Quaternion { get; set; }
        public List<double> Distances { get; set; } = new List<double>();

        public override string ToString() => Serialize();

        public string Serialize()
        {
            return $"{EulerAngles};{Quaternion};[{string.Join(",", Distances)}]";
        }

        public static bool TryParse(string serialized, out LegoCarState state)
        {
            state = null;
            var parts = serialized.Split(';');
            if (parts.Length != 3)
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

            if (!Doubles.TryParse(parts[2], out var distances))
            {
                Console.WriteLine("Bad doubles");
                return false;
            }

            state = new LegoCarState
            {
                EulerAngles = euler,
                Quaternion = quaternion,
                Distances = distances
            };
            return true;
        }
    }
}