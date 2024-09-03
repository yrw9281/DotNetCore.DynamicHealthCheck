using Microsoft.Extensions.Logging;

namespace DynamicHealthCheck.LogSeverity;

internal class LogSeverityLogLevel
{
    public LogLevel LogLevel { get; set; }
    public int MaxWarningCount { get; set; }
    public int PeriodInMinutes { get; set; }
}