using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck;

/// <summary>
///     Marker interface that links a health check instance to its configuration context type.
///     Implement this interface on context classes to automatically bind them to the corresponding
///     <see cref="IHealthCheck" /> during dynamic registration.
/// </summary>
/// <typeparam name="THealthCheck">Health check type associated with the context.</typeparam>
public interface IHealthCheckContext<THealthCheck> where THealthCheck : class, IHealthCheck
{
}
