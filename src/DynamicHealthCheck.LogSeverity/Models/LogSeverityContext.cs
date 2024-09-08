namespace DynamicHealthCheck.LogSeverity.Models;

internal class LogSeverityContext
{
    public ICollection<LogSeverityLogLevel>? LogLevels { get; init; }
}