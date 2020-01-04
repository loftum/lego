using System.Threading.Tasks;

namespace Lego.Client
{
    public interface ICarInput
    {
        Task<int> GetThrottleAsync();
        Task<int> GetSteerAngleDegAsync();
    }
}