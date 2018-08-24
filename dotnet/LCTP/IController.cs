using System.Threading.Tasks;

namespace LCTP
{
    public interface IController
    {
        Task<ResponseMessage> Execute(RequestMessage request);
    }
}