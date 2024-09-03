using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Models;

public class DynamicHealthCheckConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public string HealthCheckName { get; set; } = string.Empty;
    public HealthStatus FailureStatus { get; set; } = HealthStatus.Unhealthy;
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Transient;
    public int? TimeoutInSeconds { get; set; }
    public string[]? Tags { get; set; }
    public object? Context { get; set; }
}