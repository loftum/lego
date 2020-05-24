using System;
using System.Threading.Tasks;

namespace LCTP.Core.Client
{
    public interface ILctpClient : IDisposable
    {
        bool Connected { get; }
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendAsync(RequestMessage request);
        Func<ResponseMessage, Task> OnResponseReceived { get; set; }
    }
}