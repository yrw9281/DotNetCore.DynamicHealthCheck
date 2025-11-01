using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Abstractions;

public interface IDynamicHealthCheckConfigService<THealthCheck> where THealthCheck : IHealthCheck
{
    TContext GetContext<TContext>(HealthCheckContext healthCheckContext) where TContext : class;
    IEnumerable<TContext> GetContexts<TContext>() where TContext : class;
    HealthStatus GetFailureStatus(string serviceName);
    HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext);
}