using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Core.Logging;

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;

    private static readonly ConcurrentDictionary<string, object> _fileLocks = new();

    public FileLogger(string categoryName, string filePath)
    {
        _categoryName = categoryName;
        _filePath = filePath;

        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }
        Directory.CreateDirectory(dir);
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

        var message = formatter(state, exception);
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel,-11}] [{_categoryName}] {message}";

        if (exception != null)
        {
            entry += Environment.NewLine + exception;
        }

        var fileLock = _fileLocks.GetOrAdd(_filePath, _ => new object());
        lock (fileLock)
        {
            File.AppendAllText(_filePath, entry + Environment.NewLine);
        }
    }
}
