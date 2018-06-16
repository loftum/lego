using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Terminal.Interactive.Roslyn
{
    public class AppController : IAppController
    {
        private readonly string[] _initialUsings;
        private readonly CancellationToken _cancellationToken;
        private ScriptState _scriptState;
        private readonly object _context;

        public AppController(object context, CancellationToken cancellationToken, IEnumerable<string> usings = null)
        {
            _initialUsings = new[]
                {
                    "System",
                    "System.Linq",
                    "System.Collections",
                    "System.Collections.Generic",
                }
                .Concat(usings ?? new string[0])
                .Distinct()
                .ToArray();
            _context = context;
            _cancellationToken = cancellationToken;
            Reset().Wait();
        }

        public async Task<object> ExecuteAsync(string code, CancellationToken cancellationToken)
        {
            try
            {
                _scriptState = await _scriptState.ContinueWithAsync(code, cancellationToken: cancellationToken);
                var result = _scriptState.GetResult();
                if (result is Task t)
                {
                    await t;
                    var taskType = t.GetType();
                    if (taskType.IsGenericType)
                    {
                        return taskType.GetProperty("Result").GetValue(t);
                    }
                }
                return result;
            }
            catch (CompilationErrorException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.GetBaseException();
            }
        }

        public Task<List<CompletionData>> GetCompletions(string code, int location)
        {
            var script = _scriptState.Script.ContinueWith(code);
            script.Compile();
            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var completer = new CodeCompleter(tree, compilation, location);
            var completions = completer.GetCompletions();
            return Task.FromResult(completions.ToList());
        }

        public async Task Reset()
        {
            var options = ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
                .WithImports(_initialUsings);
            _scriptState = await CSharpScript.RunAsync("", options, cancellationToken: _cancellationToken, globals: _context);
        }
    }
}