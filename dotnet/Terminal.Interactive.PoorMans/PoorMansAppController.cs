using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal.Interactive.PoorMans
{
    public class PoorMansAppController : IAppController
    {
        private readonly object _context;
        private readonly ObjectNode _model;

        public PoorMansAppController(object context)
        {
            _model = new ObjectNode(context.GetType(), context);
            _context = context;
        }

        public Task<List<CompletionData>> GetCompletions(string code, int location)
        {
            return Task.FromResult(new List<CompletionData>());
        }

        public Task Reset()
        {
            return Task.CompletedTask;
        }

        public Task<object> ExecuteAsync(string code, CancellationToken cancellationToken)
        {
            var parts = code.Split('=');
            return parts.Length == 2 ? ExecuteAssignment(parts[0], parts[1], cancellationToken) : ExecuteStatement(code, cancellationToken);
        }

        private async Task<object> ExecuteStatement(string code, CancellationToken cancellationToken)
        {
            var member = _model.Traverse(code.Split('.'));

            var value = member.GetValue();
            if (value is Task t)
            {
                await t;
                var taskType = t.GetType();
                if (taskType.IsGenericType)
                {
                    return taskType.GetProperty("Result").GetValue(t);
                }
            }
            return value;
        }

        private Task<object> ExecuteAssignment(string left, string right, CancellationToken cancellationToken)
        {
            if (!(_model.Traverse(left.Split('.')) is PropertyInfo property))
            {
                return Task.FromResult((object)$"Unknown property {left}");
            }
            var value = Convert.ChangeType(right, property.PropertyType);
            property.SetValue(_context, value);
            return Task.FromResult((object)"OK");
        }
    }
}