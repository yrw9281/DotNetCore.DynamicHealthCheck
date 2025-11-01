using DynamicHealthCheck.Abstractions;

namespace DynamicHealthCheck.Configuration;

internal sealed class HealthCheckContextBinding : IHealthCheckContextBinding
{
    public HealthCheckContextBinding(Type healthCheckType, Type contextType)
    {
        if (healthCheckType is null) throw new ArgumentNullException(nameof(healthCheckType));
        if (contextType is null) throw new ArgumentNullException(nameof(contextType));

        HealthCheckType = healthCheckType;
        ContextType = contextType;
    }

    public Type HealthCheckType { get; }

    public Type ContextType { get; }
}
