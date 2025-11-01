using DynamicHealthCheck.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Abstractions;

internal interface IDynamicHealthCheckConfigurationProvider
{
    DynamicHealthCheckRoot? GetRoot();

    DynamicHealthCheckConfig GetHealthCheckConfig<THealthCheck>(string serviceName)
        where THealthCheck : class, IHealthCheck;

    TContext GetHealthCheckContext<THealthCheck, TContext>(string serviceName)
        where THealthCheck : class, IHealthCheck
        where TContext : class;

    IEnumerable<TContext> GetHealthCheckContexts<THealthCheck, TContext>()
        where THealthCheck : class, IHealthCheck
        where TContext : class;
}
