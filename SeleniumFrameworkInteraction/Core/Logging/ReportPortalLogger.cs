using Microsoft.Extensions.Logging;
using ReportPortal.Shared;
using ReportPortal.Shared.Execution.Logging;

namespace Core.Logging;

public class ReportPortalLogger : ILogger
{
    private readonly string _categoryName;

    public ReportPortalLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            if (Context.Current == null)
            {
                return;
            }

            var message = formatter(state, exception);
            var fullMessage = $"[{_categoryName}] {message}";

            if (exception != null)
            {
                fullMessage += Environment.NewLine + exception;
            }

            var rpLogLevel = MapLogLevel(logLevel);

            switch (rpLogLevel)
            {
                case LogMessageLevel.Trace:
                    Context.Current.Log.Trace(fullMessage);
                    break;
                case LogMessageLevel.Debug:
                    Context.Current.Log.Debug(fullMessage);
                    break;
                case LogMessageLevel.Info:
                    Context.Current.Log.Info(fullMessage);
                    break;
                case LogMessageLevel.Warning:
                    Context.Current.Log.Warn(fullMessage);
                    break;
                case LogMessageLevel.Error:
                    Context.Current.Log.Error(fullMessage);
                    break;
                case LogMessageLevel.Fatal:
                    Context.Current.Log.Fatal(fullMessage);
                    break;
            }
        }
        catch(Exception)
        {
            // Intentionally ignored.
            // Logging failures must never affect test
        }
    }

    private static LogMessageLevel MapLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogMessageLevel.Trace,
            LogLevel.Debug => LogMessageLevel.Debug,
            LogLevel.Information => LogMessageLevel.Info,
            LogLevel.Warning => LogMessageLevel.Warning,
            LogLevel.Error => LogMessageLevel.Error,
            LogLevel.Critical => LogMessageLevel.Fatal,
            _ => LogMessageLevel.Info
        };
    }
}