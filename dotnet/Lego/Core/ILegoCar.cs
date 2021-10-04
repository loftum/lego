using System.Threading.Tasks;
using Maths;

namespace Lego.Core
{
    public interface ILegoCar
    {
        void SetThrottle(int speed);
        void SetSteerAngle(int angle);
        void Reset();
        LegoCarState GetState();
        Task StopEngineAsync();
        Task StartEngineAsync();
    }

    public interface IRedCar : ILegoCar
    {
        Double3 GetEulerAngles();
        Quatd GetQuaternion();
        ILight LeftBlinker { get; }
        ILight RightBlinker { get; }
        ILight Headlights { get; }
    }
}