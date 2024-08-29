using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeepHealthCheck.Services;

public class DeepHealthCheckConfigService<THealthCheck>(IConfiguration configuration)
    : IDeepHealthCheckConfigService<THealthCheck>
    where THealthCheck : class, IHealthCheck
{
    public TContext GetContext<TContext>(HealthCheckContext healthCheckContext)
        where TContext : class
        => ConfigurationManager.GetHealthCheckContext<THealthCheck, TContext>(configuration, healthCheckContext.Registration.Name);
    
    public IEnumerable<TContext> GetContexts<TContext>() where TContext : class
        => ConfigurationManager.GetHealthCheckContexts<THealthCheck, TContext>(configuration);

    public HealthStatus GetFailureStatus(string serviceName)
        => ConfigurationManager.GetHealthCheckConfig<THealthCheck>(configuration, serviceName)
            .FailureStatus;
    
    public HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext)
        => ConfigurationManager.GetHealthCheckConfig<THealthCheck>(configuration, healthCheckContext.Registration.Name)
            .FailureStatus;
}