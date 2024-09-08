using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck;

public static class HealthCheckFactory
{
    public static IHealthCheck CreateHealthCheck(IServiceProvider serviceProvider, Type healthCheckType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                return (IHealthCheck)serviceProvider.GetRequiredService(healthCheckType);
            case ServiceLifetime.Transient:
                return (IHealthCheck)ActivatorUtilities.CreateInstance(serviceProvider, healthCheckType);
            case ServiceLifetime.Scoped:
                using (var scope = serviceProvider.CreateScope())
                {
                    return (IHealthCheck)scope.ServiceProvider.GetRequiredService(healthCheckType);
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}