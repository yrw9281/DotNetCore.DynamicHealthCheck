using Microsoft.Extensions.Logging;

namespace DeepHealthCheck.HealthChecks.LogSeverity;

public class LogSeverityLoggerProvider(ILogSeverityLogger logger) : ILogSeverityLoggerProvider
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