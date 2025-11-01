using DynamicHealthCheck;
using DynamicHealthCheck.LogSeverity;

namespace DynamicHealthCheck.LogSeverity.Models;

internal class LogSeverityContext : IHealthCheckContext<LogSeverityHealthCheck>
{
    public ICollection<LogSeverityLogLevel>? LogLevels { get; init; }
}