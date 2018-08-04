using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Interactive;

namespace PiconTest
{
    public class DictionaryAppController : IAppController
    {
        private readonly IList<Command> _commands;

        public DictionaryAppController(IList<Command> commands)
        {
            _commands = commands;
        }

        public Task<object> ExecuteAsync(string code, CancellationToken cancellationToken)
        {
            var action = (from c in _commands
                let match = c.Regex.Match(code)
                where match.Success
                select c.GetAction(match)).FirstOrDefault();
            if (action == null)
            {
                Console.WriteLine("Didn't undestand. Sorry.");
                return Task.FromResult<object>(null);
            }
            return action();
        }

        public Task<List<CompletionData>> GetCompletions(string code, int location)
        {
            return Task.FromResult(new List<CompletionData>());
        }

        public Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}