using DeepHealthCheck.HealthChecks.LogSeverity.Models;
using DeepHealthCheck.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeepHealthCheck.HealthChecks.LogSeverity;

internal class LogSeverityHealthCheck(
    IMemoryCache memoryCache,
    IDeepHealthCheckConfigService<LogSeverityHealthCheck> deepHealthCheckConfigService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var logSeverity = deepHealthCheckConfigService.GetContext<LogSeverityContext>(context);
        var failureStatus = deepHealthCheckConfigService.GetFailureStatus(context);

        try
        {
            foreach (var logLevelConfig in logSeverity.LogLevels!)
            {
                var logLevelKey = $"{Constants.LOG_KEY_PREFIX}{Constants.SEPARATOR}{logLevelConfig.LogLevel}";
                var logKeys = memoryCache.Get<List<string>>(logLevelKey);
                var logs = new List<string>();
                if (logKeys == null) continue;
                foreach (var logKey in logKeys)
                {
                    if (!memoryCache.TryGetValue(logKey, out string? logMessage)) continue;
                    if (!string.IsNullOrWhiteSpace(logMessage)) logs.Add(logMessage);
                }

                if (logs.Count >= logLevelConfig.MaxWarningCount)
                    return Task.FromResult(new HealthCheckResult(failureStatus));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, exception: ex));
        }
    }
}