using Convenient.Gooday.Logging;

namespace LCTP.Logging
{
    public class LogAdapter : Convenient.Gooday.Logging.ILogger
    {
        private readonly ILogger _logger;

        public LogAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public void Write(LogImportance importance, LogMessageType type, object message)
        {
            _logger.Write(Map(importance), Map(type), message);
        }

        public static LogAdapter For(string name)
        {
            return new LogAdapter(Log.For(name));
        }

        private static MessageType Map(LogMessageType type)
        {
            return type switch
            {
                LogMessageType.Debug => MessageType.Debug,
                LogMessageType.Info => MessageType.Info,
                LogMessageType.Warning => MessageType.Warning,
                LogMessageType.Error => MessageType.Error,
                _ => MessageType.Info
            };
        }

        private static Importance Map(LogImportance importance)
        {
            return importance switch
            {
                LogImportance.Trace => Importance.Trace,
                LogImportance.Debug => Importance.Debug,
                LogImportance.Normal => Importance.Normal,
                LogImportance.Important => Importance.Important,
                _ => Importance.Normal
            };
        }
    }
}