using Maths;

namespace Lego.Client
{
    public interface IRotationProvider
    {
        Double3 GetRotation();
        Quatd GetQuaternion();
    }
}