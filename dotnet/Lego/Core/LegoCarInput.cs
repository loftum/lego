using System;

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
}