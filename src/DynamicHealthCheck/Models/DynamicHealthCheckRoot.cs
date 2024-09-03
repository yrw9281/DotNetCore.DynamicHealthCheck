namespace DynamicHealthCheck.Models;

public class DynamicHealthCheckRoot
{
    public bool Disabled { get; init; }
    public ICollection<DynamicHealthCheckConfig>? HealthChecks { get; init; }
}