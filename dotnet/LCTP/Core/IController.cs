using System.Threading.Tasks;

namespace LCTP.Core
{
    public interface IController
    {
        Task<ResponseMessage> Execute(RequestMessage request);
        void ConnectionOpened();
        void ConnectionClosed();
    }
}