using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal.Interactive
{
    public interface IAppController
    {
        Task<object> ExecuteAsync(string code, CancellationToken cancellationToken);
        Task<List<CompletionData>> GetCompletions(string code, int location);
        Task Reset();
    }
}