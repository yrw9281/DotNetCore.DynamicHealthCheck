namespace DeepHealthCheck.Models;

public class DeepHealthCheckRoot
{
    public bool Disabled { get; init; }
    public ICollection<DeepHealthCheckConfig>? HealthChecks { get; init; }
}