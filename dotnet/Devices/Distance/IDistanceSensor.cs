using Devices.Distance.Sharp.GP2Y0A41SK0F;

namespace Devices.Distance
{
    public interface IDistanceSensor
    {
        double MinValue { get; }
        double MaxValue { get; }
        CalculatedValue GetCm();
    }
}