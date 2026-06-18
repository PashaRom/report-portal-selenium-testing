using Core.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Logging;

public static class TestLoggerFactory
{
    private static ILoggerFactory? _instance;
    private static readonly object _lock = new();

    public static ILoggerFactory Instance
    {
        get
        {
            if (_instance != null) return _instance;
            lock (_lock)
            {
                _instance ??= Create(AppConfiguration.LogSettings);
            }
            return _instance;
        }
    }

    public static ILogger<T> CreateLogger<T>() => Instance.CreateLogger<T>();

    public static ILogger CreateLogger(string categoryName) => Instance.CreateLogger(categoryName);

    private static ILoggerFactory Create(LogSettings settings)
    {
        if (!System.Enum.TryParse<LogLevel>(settings.MinLevel, ignoreCase: true, out var minLevel))
            minLevel = LogLevel.Information;

        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(minLevel);

            if (settings.EnableConsole)
                builder.AddConsole();

            if (settings.EnableFile)
                builder.AddProvider(new FileLoggerProvider(settings.FilePath));
        });
    }
}
