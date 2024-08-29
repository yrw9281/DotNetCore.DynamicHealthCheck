using DeepHealthCheck.HealthChecks.LogSeverity.Models;
using DeepHealthCheck.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeepHealthCheck.HealthChecks.LogSeverity;

internal class LogSeverityLogger(
    IMemoryCache memoryCache,
    IDeepHealthCheckConfigService<LogSeverityHealthCheck> configService) : ILogSeverityLogger
{
    private readonly List<LogSeverityContext> _logSeverities = configService.GetContexts<LogSeverityContext>().ToList();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        foreach (var logSeverityContext in _logSeverities)
        {
            var logLevelConfig = logSeverityContext.LogLevels?.FirstOrDefault(c => c.LogLevel == logLevel);
            if (logLevelConfig == null) continue;

            // Store log to memory cache
            var logMessage = formatter(state, exception);
            if (logMessage.Contains(Constants.HEALTH_CHECK_ERROR_MESSAGE_KEYWORD)) continue;
            var cacheKey = $"{logLevel}{Constants.SEPARATOR}{eventId.Id}";
            memoryCache.Set(cacheKey, logMessage, TimeSpan.FromMinutes(logLevelConfig.PeriodInMinutes));

            // Store log keys list to memory cache
            var logKeysKey = $"{Constants.LOG_KEY_PREFIX}{Constants.SEPARATOR}{logLevel}";
            var logKeys = memoryCache.GetOrCreate(logKeysKey, entry => new List<string>()) ?? [];
            logKeys.RemoveAll(key => !memoryCache.TryGetValue(key, out _));
            logKeys.Add(cacheKey);
            if (logKeys.Count > logLevelConfig.MaxWarningCount)
                logKeys.RemoveAt(0);
            memoryCache.Set(logKeysKey, logKeys);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logSeverities.Any(s =>
            s.LogLevels != null &&
            s.LogLevels.Any(l => l.LogLevel == logLevel));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}