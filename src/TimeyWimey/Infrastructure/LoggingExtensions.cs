namespace TimeyWimey.Infrastructure;

public static class LoggingExtensions
{
    public static void LogDebug<T>(this ILogger<T> logger, Func<string> log)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(log());
        }
    }

}