namespace DynamicHealthCheck.Abstractions;

internal interface IHealthCheckContextBinding
{
    Type HealthCheckType { get; }
    Type ContextType { get; }
}
