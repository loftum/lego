using System.Linq;

namespace LCTP
{
    public class ResponseMessage
    {
        public int StatusCode { get; set; } = 200;
        public string Content { get; set; }

        public string Format()
        {
            return $"{StatusCode} {Content}";
        }

        public override string ToString()
        {
            return Format();
        }

        public static ResponseMessage Parse(string response)
        {
            var parts = response.Split(' ');
            return new ResponseMessage
            {
                StatusCode = int.TryParse(parts[0], out var s) ? s : 0,
                Content = string.Join(" ", parts.Skip(1))
            };
        }

        public static ResponseMessage BadRequest(string message = null)
        {
            return new ResponseMessage
            {
                StatusCode = 400,
                Content = message
            };
        }

        public static ResponseMessage Ok(object content = null)
        {
            return new ResponseMessage
            {
                StatusCode = 200,
                Content = content?.ToString() ?? "OK"
            };
        }
    }
}