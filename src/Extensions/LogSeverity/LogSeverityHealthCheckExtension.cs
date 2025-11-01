using DynamicHealthCheck.Abstractions;
using DynamicHealthCheck.LogSeverity.Infrastructure;
using DynamicHealthCheck.LogSeverity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DynamicHealthCheck.LogSeverity;

public static class LogSeverityHealthCheckExtension
{
    public static IHealthChecksBuilder AddLogSeverity(this IHealthChecksBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.TryAddSingleton<ILogSeverityLogger, LogSeverityLogger>();
        builder.Services.TryAddSingleton<ILogSeverityLoggerProvider, LogSeverityLoggerProvider>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddlewarePipelineContributor, LogSeverityLoggerContributor>());

        return builder;
    }
}