using System.Text.RegularExpressions;

namespace LCTP.Core.Routing
{
    public static class MatchExtensions
    {
        public static int GetInt(this Match match, string groupName)
        {
            var g = match.Groups[groupName];
            return int.Parse(g.Value);
        }

        public static int GetInt(this Match match, int groupNumber)
        {
            var g = match.Groups[groupNumber];
            return int.Parse(g.Value);
        }
    }
}