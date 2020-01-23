using Maths;

namespace Lego.Client
{
    public interface IRotationProvider
    {
        Double3 GetEulerAngles();
        Quatd GetQuaternion();
    }
}