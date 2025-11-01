using System.Collections.Concurrent;
using DynamicHealthCheck.Abstractions;

namespace DynamicHealthCheck.Configuration;

internal sealed class HealthCheckContextRegistry : IHealthCheckContextRegistry
{
    private readonly ConcurrentDictionary<Type, Type> _mappings = new();

    public HealthCheckContextRegistry(IEnumerable<IHealthCheckContextBinding> bindings)
    {
        foreach (var binding in bindings)
        {
            TryAdd(binding.HealthCheckType, binding.ContextType);
        }
    }

    public bool TryGet(Type healthCheckType, out Type? contextType)
    {
        if (healthCheckType is null) throw new ArgumentNullException(nameof(healthCheckType));
        return _mappings.TryGetValue(healthCheckType, out contextType);
    }

    public bool TryAdd(Type healthCheckType, Type contextType)
    {
        if (healthCheckType is null) throw new ArgumentNullException(nameof(healthCheckType));
        if (contextType is null) throw new ArgumentNullException(nameof(contextType));

        return _mappings.TryAdd(healthCheckType, contextType);
    }
}
