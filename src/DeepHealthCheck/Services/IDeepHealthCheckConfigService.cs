using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeepHealthCheck.Services;

public interface IDeepHealthCheckConfigService<THealthCheck> where THealthCheck : IHealthCheck
{
    TContext GetContext<TContext>(HealthCheckContext healthCheckContext) where TContext : class;
    IEnumerable<TContext> GetContexts<TContext>() where TContext : class;
    HealthStatus GetFailureStatus(string serviceName);
    HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext);
}