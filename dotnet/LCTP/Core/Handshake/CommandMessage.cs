namespace LCTP.Core.Handshake
{
    class CommandMessage
    {
        public string Command { get; set; }

        public static CommandMessage Decode(string value)
        {
            if (value == null)
            {
                return null;
            }
            var parts = value.Split(';');
            if (parts.Length != 1)
            {
                return null;
            }

            return new CommandMessage
            {
                Command = parts[0]
            };
        }

        public string Encode()
        {
            return Command;
        }
    }
}