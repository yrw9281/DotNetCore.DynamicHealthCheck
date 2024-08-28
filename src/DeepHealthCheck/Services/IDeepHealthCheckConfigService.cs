using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeepHealthCheck.Services;

public interface IDeepHealthCheckConfigService<THealthCheck> where THealthCheck : IHealthCheck
{
    TContext GetContext<TContext>() where TContext : class;
    TContext GetContext<TContext>(HealthCheckContext healthCheckContext) where TContext : class;
    HealthStatus GetFailureStatus(string serviceName);
    HealthStatus GetFailureStatus(HealthCheckContext healthCheckContext);
}