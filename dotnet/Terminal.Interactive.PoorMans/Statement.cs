using System.Linq;
using System.Text.RegularExpressions;

namespace Terminal.Interactive.PoorMans
{
    public class Statement
    {
        private const string Pattern = @"^(?<name>[a-zA-Z0-9]+)(\({1}(?<args>.*)\){1})?$";
        public string Name { get; }
        public object[] Arguments { get; }
        public bool IsMethodCall { get; }

        public Statement(string raw)
        {
            var match = Regex.Match(raw, Pattern);
            Name = match.Groups["name"].Value;
            var args = match.Groups["args"];
            IsMethodCall = args.Success;
            Arguments = args.Success
                ? args.Value.Split(',')
                    .Select(v => ParameterParser.Parse(v.Trim()))
                    .ToArray()
                : new object[0];
        }
    }
}