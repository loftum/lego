using System.Threading;
using System.Threading.Tasks;

namespace Devices.PixArt
{
    public interface IFlowSensor
    {
        Task<(short, short)> GetMotionAsync(CancellationToken cancellationToken);
    }
}