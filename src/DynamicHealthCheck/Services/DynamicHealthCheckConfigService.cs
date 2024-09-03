using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Services;

public class DynamicHealthCheckConfigService<THealthCheck>(IConfiguration configuration)
    : IDynamicHealthCheckConfigService<THealthCheck>
    where THealthCheck : class, IHealthCheck
{
    public TContext GetContext<TContext>(HealthCheckContext healthCheckContext)
        where TContext : class
    {
        return ConfigurationManager.GetHealthCheckContext<THealthCheck, TContext>(configuration,
            healthCheckContext.Registration.Name);
    }

    public IEnumerable<TContext> GetContexts<TContext>() where TContext : class
    {
        return ConfigurationManager.GetHealthCheckContexts<THealthCheck, TContext>(configuration);
    }

    public HealthStatus GetFailureStatus(string serviceName)
    {
        return ConfigurationManager.GetHealthCheckConfig<THealthCheck>(configuration, serviceName)
            .FailureStatus;
    }

    public HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext)
    {
        return ConfigurationManager
            .GetHealthCheckConfig<THealthCheck>(configuration, healthCheckContext.Registration.Name)
            .FailureStatus;
    }
}