using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiconTest
{
    public class Command
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public Func<Match, Task<object>> Execute { get; }

        public Func<Task<object>> GetAction(Match match)
        {
            return () => Execute(match);
        }

        public Command(string pattern, Func<Match, Task<object>> execute)
        {
            Regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Pattern = pattern;
            Execute = execute;
        }

        public override string ToString()
        {
            return Pattern;
        }
    }
}