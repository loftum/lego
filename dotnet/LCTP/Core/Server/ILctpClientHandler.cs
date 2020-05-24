using System;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Core.Server
{
    public interface ILctpClientHandler : IDisposable
    {
        Task Handle(CancellationToken cancellationToken);
    }
}