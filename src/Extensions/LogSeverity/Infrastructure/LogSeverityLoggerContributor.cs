using DynamicHealthCheck.Abstractions;
using DynamicHealthCheck.LogSeverity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace DynamicHealthCheck.LogSeverity.Infrastructure;

internal sealed class LogSeverityLoggerContributor(ILoggerFactory loggerFactory, ILogSeverityLoggerProvider loggerProvider)
    : IMiddlewarePipelineContributor
{
    public void Configure(IApplicationBuilder app)
    {
        loggerFactory.AddProvider(loggerProvider);
    }
}
