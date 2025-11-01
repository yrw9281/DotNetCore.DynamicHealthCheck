# DotNetCore.DynamicHealthCheck

Auto dynamic health check wiring for ASP.NET Core services.

## Features

- Discovers every `IHealthCheck` in loaded assemblies and registers them according to configuration.
- Supports per-check lifetimes, failure statuses, tags, and timeouts driven by strongly typed options.
- Supplies custom context objects to health checks via `IDynamicHealthCheckConfigService<T>`.
- Provides middleware that surfaces a `/health` endpoint and lets extensions plug extra middleware in.
- Includes an optional log-severity health check that monitors cached log events.

## Project Layout

- `src/DynamicHealthCheck` – core library with configuration manager, factory, and middleware helpers.
- `src/Extensions/LogSeverity` – extension package offering log severity monitoring.
- `samples/DynamicHealthCheck.Demo` – minimal API sample showcasing configuration and usage.

## Quick Start

Prerequisite: .NET 8 SDK.

1. Reference the library (NuGet package or project reference).
1. Register dynamic health checks in `Program.cs`:

```csharp
builder.Services
    .AddDynamicHealthCheck(builder.Configuration)
    .AddLogSeverity(); // optional extension

var app = builder.Build();
app.UseDynamicHealthCheck(); // defaults to /health
```

1. Configure checks in `appsettings.json`:

```json
{
  "DynamicHealthCheck": {
    "HealthChecks": [
      {
        "ServiceName": "LogSeverity",
        "HealthCheckName": "LogSeverityHealthCheck",
        "FailureStatus": "Unhealthy",
        "ServiceLifetime": "Singleton",
        "TimeoutInSeconds": 30,
        "Tags": ["logging"],
        "Context": {
          "LogLevels": [
            { "LogLevel": "Error", "MaxWarningCount": 3, "PeriodInMinutes": 1 }
          ]
        }
      }
    ]
  }
}
```

### Configuration Notes

- `Disabled` disables all dynamic registrations when set to `true`.
- `ServiceName` is the registration key exposed to the health check pipeline.
- `HealthCheckName` must match the class name of the `IHealthCheck` implementation.
- `ServiceLifetime` controls how the check is registered in DI.
- `Context` binds to a strongly typed class via `BindHealthCheckContext<THealthCheck, TContext>()`.

### Authoring Custom Health Checks

1. Implement `IHealthCheck` as usual.
1. Implement `IHealthCheckContext<MyCheck>` on your context class so it is automatically discovered.
1. Inject `IDynamicHealthCheckConfigService<MyCheck>` to resolve contexts and failure statuses at runtime.

## Demo Application

Run the sample to see dynamic registration and the log severity check in action:

```bash
dotnet run --project samples/DynamicHealthCheck.Demo
```

Visit `/health` for overall status. Use `/log/logWarning`, `/log/logError`, or `/log/logCritical` to generate log entries and observe how the log severity health check reacts.

## License

MIT License – see `LICENSE` for details.
