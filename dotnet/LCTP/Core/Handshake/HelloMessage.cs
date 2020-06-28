namespace LCTP.Core.Handshake
{
    class HelloMessage
    {
        public string Name { get; set; }

        public static HelloMessage Decode(string value)
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

            return new HelloMessage
            {
                Name = parts[0]
            };
        }

        public string Encode()
        {
            return Name;
        }
    }
}