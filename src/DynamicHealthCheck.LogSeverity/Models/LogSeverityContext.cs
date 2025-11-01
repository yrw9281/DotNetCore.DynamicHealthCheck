using DynamicHealthCheck.Abstractions;

namespace DynamicHealthCheck.LogSeverity.Models;

internal class LogSeverityContext : IHealthCheckContext<LogSeverityHealthCheck>
{
    public ICollection<LogSeverityLogLevel>? LogLevels { get; init; }
}