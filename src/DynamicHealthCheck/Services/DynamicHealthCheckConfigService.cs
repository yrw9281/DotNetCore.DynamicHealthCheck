using DynamicHealthCheck.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Services;

internal class DynamicHealthCheckConfigService<THealthCheck>(IDynamicHealthCheckConfigurationProvider configurationProvider)
    : IDynamicHealthCheckConfigService<THealthCheck>
    where THealthCheck : class, IHealthCheck
{
    public TContext GetContext<TContext>(HealthCheckContext healthCheckContext)
        where TContext : class
    {
        return configurationProvider.GetHealthCheckContext<THealthCheck, TContext>(healthCheckContext.Registration.Name);
    }

    public IEnumerable<TContext> GetContexts<TContext>() where TContext : class
    {
        return configurationProvider.GetHealthCheckContexts<THealthCheck, TContext>();
    }

    public HealthStatus GetFailureStatus(string serviceName)
    {
        return configurationProvider.GetHealthCheckConfig<THealthCheck>(serviceName).FailureStatus;
    }

    public HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext)
    {
        return configurationProvider
            .GetHealthCheckConfig<THealthCheck>(healthCheckContext.Registration.Name)
            .FailureStatus;
    }
}