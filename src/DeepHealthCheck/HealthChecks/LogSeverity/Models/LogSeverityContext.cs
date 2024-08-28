namespace DeepHealthCheck.HealthChecks.LogSeverity.Models;

internal class LogSeverityContext 
{
    public ICollection<LogSeverityLogLevel>? LogLevels { get; init; }
}