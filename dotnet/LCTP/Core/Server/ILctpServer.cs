using System;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Core.Server
{
    public interface ILctpServer : IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}