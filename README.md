# DynamicHealthCheck

A .NET 8 library for dynamic health check configuration and injection.

## Key Feature: Dynamic Health Check Context Binding

This project leverages the generic marker interface `IHealthCheckContext<THealthCheck>` to associate health check implementations with their configuration contexts. By implementing this interface on your context classes, you enable automatic binding and dynamic registration of health check configurations.

### How It Works

- **Define a context class** for your health check and implement `IHealthCheckContext<THealthCheck>`.
- **Register health checks dynamically**: The library scans for context classes implementing the interface and binds them to their corresponding health check types.
- **Inject and use configs**: Health check configurations are injected and managed automatically, simplifying setup and maintenance.

### Example Usage

```csharp
public class MyHealthCheckContext : IHealthCheckContext<MyHealthCheck>
{
    // Configuration properties
}
```

This enables your health check (`MyHealthCheck`) to be registered and configured dynamically, without manual wiring.

## Configuration Models

The library provides flexible configuration models for health checks:

- `DynamicHealthCheckConfig`: Defines individual health check settings, including service name, health check name, failure status, service lifetime, timeout, tags, and context.
- `DynamicHealthCheckRoot`: Aggregates multiple health check configs and supports global enable/disable.

These models allow you to declaratively control health check registration and behavior via code or configuration files.

## Benefits

- Strongly-typed configuration binding
- Automatic context-health check association
- Flexible and declarative config models
- Simplified dynamic registration and injection

## License

This project is licensed under the MIT License.
