namespace DynamicHealthCheck.Abstractions;

internal interface IHealthCheckContextRegistry
{
    bool TryGet(Type healthCheckType, out Type? contextType);

    bool TryAdd(Type healthCheckType, Type contextType);
}
