using System.Text.RegularExpressions;

namespace Terminal.Interactive.PoorMans
{
    public static class ParameterParser
    {
        private const string LiteralPattern = "^\"{1}(?<string>.*)\"{1}$";
        private const string CharPattern = "^\'{1}(?<char>.{1})\'{1}$";

        public static object Parse(string input)
        {
            switch (input)
            {
                case null:
                    return null;
                case "true":
                    return true;
                case "false":
                    return false;
            }
            var stringMatch = Regex.Match(input, LiteralPattern);
            if (stringMatch.Success)
            {
                return stringMatch.Groups["string"].Value;
            }

            var charMatch = Regex.Match(input, CharPattern);
            if (charMatch.Success)
            {
                return charMatch.Groups["char"].Value;
            }

            if (byte.TryParse(input, out var b))
            {
                return b;
            }

            if (int.TryParse(input, out var i))
            {
                return i;
            }

            if (double.TryParse(input, out var d))
            {
                return d;
            }
            return input;
        }
    }
}