using Microsoft.Extensions.Logging;

namespace Core.Logging;

public class ReportPortalLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new ReportPortalLogger(categoryName);

    public void Dispose() { }
}