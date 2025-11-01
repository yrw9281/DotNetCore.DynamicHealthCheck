using Microsoft.Extensions.Logging;

namespace DynamicHealthCheck.LogSeverity.Services;

internal class LogSeverityLoggerProvider(ILogSeverityLogger logger) : ILogSeverityLoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return logger;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}