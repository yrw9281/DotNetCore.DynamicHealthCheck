namespace DynamicHealthCheck.LogSeverity;

internal class LogSeverityContext
{
    public ICollection<LogSeverityLogLevel>? LogLevels { get; init; }
}