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
            return $"{EulerAngles};[{string.Join(",", Distances)}]";
        }

        public static bool TryParse(string serialized, out LegoCarState state)
        {
            state = null;
            var parts = serialized.Split(';');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!Double3.TryParse(parts[0], out var orientation))
            {
                return false;
            }

            if (!Doubles.TryParse(parts[1], out var distances))
            {
                return false;
            }

            state = new LegoCarState
            {
                EulerAngles = orientation,
                Distances = distances
            };
            return true;
        }
    }
}