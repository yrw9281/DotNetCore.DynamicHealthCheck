using DynamicHealthCheck.LogSeverity.Models;
using DynamicHealthCheck.LogSeverity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DynamicHealthCheck.LogSeverity;

public static class LogSeverityHealthCheckExtension
{
    public static IHealthChecksBuilder AddLogSeverity(this IHealthChecksBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.TryAddSingleton<ILogSeverityLogger, LogSeverityLogger>();
        builder.Services.TryAddSingleton<ILogSeverityLoggerProvider, LogSeverityLoggerProvider>();
        
        builder.BindHealthCheckContext<LogSeverityHealthCheck, LogSeverityContext>();

        MiddlewareManager.RegisterMiddleware(applicationBuilder =>
        {
            var provider = applicationBuilder.ApplicationServices.GetRequiredService<ILogSeverityLoggerProvider>();
            var loggerFactory = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(provider);
        });

        return builder;
    }
}