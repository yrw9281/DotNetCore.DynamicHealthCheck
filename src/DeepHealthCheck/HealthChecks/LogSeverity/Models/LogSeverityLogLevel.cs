using Microsoft.Extensions.Logging;

namespace DeepHealthCheck.HealthChecks.LogSeverity.Models;

internal class LogSeverityLogLevel
{
    public LogLevel LogLevel { get; set; }
    public int MaxWarningCount { get; set; }
    public int PeriodInMinutes { get; set; }
}