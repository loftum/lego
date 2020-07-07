namespace Maths.Logging
{
    public interface ILogger
    {
        void Write(Importance importance, MessageType type, object message);
    }

    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, object message) => logger.Write(Importance.Trace, MessageType.Debug, message);
        public static void Debug(this ILogger logger, object message) => logger.Write(Importance.Debug, MessageType.Debug, message);
        public static void Info(this ILogger logger, object message) => logger.Write(Importance.Normal, MessageType.Info, message);
        public static void Warn(this ILogger logger, object message) => logger.Write(Importance.Normal, MessageType.Warning, message);
        public static void Error(this ILogger logger, object message) => logger.Write(Importance.Important, MessageType.Error, message);
    }
}