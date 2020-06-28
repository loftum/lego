namespace LCTP.Core.Handshake
{
    public class ConnectMessage
    {
        public int Port { get; set; }

        public static ConnectMessage Decode(string value)
        {
            if (value == null || !int.TryParse(value, out var port))
            {
                return null;
            }

            return new ConnectMessage
            {
                Port = port
            };
        }

        public string Encode()
        {
            return $"{Port}";
        }
    }
}