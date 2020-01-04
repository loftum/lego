using System.Collections.Generic;
using Maths;

namespace Lego.Core
{
    public class LegoCarState
    {
        public Vector3 Orientation { get; set; }
        public List<double> Distances { get; set; } = new List<double>();

        public override string ToString() => Serialize();

        public string Serialize()
        {
            return $"{Orientation};[{string.Join(",", Distances)}]";
        }

        public static bool TryParse(string serialized, out LegoCarState state)
        {
            state = null;
            var parts = serialized.Split(';');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!Vector3.TryParse(parts[0], out var orientation))
            {
                return false;
            }

            if (!Doubles.TryParse(parts[1], out var distances))
            {
                return false;
            }

            state = new LegoCarState
            {
                Orientation = orientation,
                Distances = distances
            };
            return true;
        }
    }
}