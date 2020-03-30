using System;
using System.Collections.Generic;
using Maths;

namespace Lego.Core
{
    public class LegoCarInput
    {
        public int Throttle { get; set; }
        public int SteerAngle { get; set; }
        
        public string Serialize()
        {
            return $"{Throttle};{SteerAngle}";
        }

        public static bool TryParse(string serialized, out LegoCarInput input)
        {
            input = null;
            var parts = serialized.Split(';');
            if (parts.Length != 2)
            {
                Console.WriteLine("LegoCarInput: Bad length");
                return false;
            }

            if (!int.TryParse(parts[0], out var throttle))
            {
                Console.WriteLine($"LegoCarInput: Bad throttle: {parts[0]}");
                return false;
            }

            if (!int.TryParse(parts[1], out var steerAngle))
            {
                Console.WriteLine($"LegoCarInput: Bad steerAngle: {parts[1]}");
                return false;
            }

            input = new LegoCarInput
            {
                Throttle = throttle,
                SteerAngle = steerAngle
            };
            return true;
        }
    }
    
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