using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Terminal.Interactive
{
    public static class Prettifier
    {
        public static string Pretty(this object o)
        {
            switch (o)
            {
                case null: return null;
                case string s: return s;
                case Task t: return "";
                case WebException we: return Format(we);
                case Exception e: return e.ToString();
                default: return JsonConvert.SerializeObject(o, Formatting.Indented);
            }
        }

        private static string Format(WebException we)
        {
            var builder = new StringBuilder().Append(we).AppendLine();

            if (we.Response is HttpWebResponse response && response.ContentLength > 0)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var txt = reader.ReadToEnd();
                    builder.AppendLine("Response:").AppendLine(txt);
                }
            }
            return builder.ToString();
        }
    }
}