using DeepHealthCheck.HealthChecks.LogSeverity;
using DeepHealthCheck.HealthChecks.LogSeverity.Models;
using DeepHealthCheck.Models;
using DeepHealthCheck.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace DeepHealthCheck;

public static class DeepHealthCheckExtension
{
    /// <summary>
    /// Registers dynamic health checks based on the provided configuration.
    /// This method scans the application's assemblies for implementations of <see cref="IHealthCheck"/> and registers them.
    /// It also injects dependencies for generic health checks if required, and configures each health check according to the specified settings.
    /// </summary>
    /// <param name="services">The service collection to which the health checks will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve health check settings.</param>
    /// <param name="configSection">The name of the configuration section containing health check settings. Defaults to <see cref="Constants.DEFAULT_CONFIG_SECTION_NAME"/>.</param>
    /// <returns>An <see cref="IHealthChecksBuilder"/> instance for further health check configuration.</returns>
    public static IHealthChecksBuilder AddDeepHealthCheck(this IServiceCollection services,
        IConfiguration configuration,
        string configSection = Constants.DEFAULT_CONFIG_SECTION_NAME)
    {
        ConfigurationManager.ConfigSection = configSection;

        var healthCheckBuilder = services.AddHealthChecks();

        var rootConfig = ConfigurationManager.Get(configuration);

        // Add normal HealthChecks only if disabled.
        if (IsDisabled(rootConfig)) return healthCheckBuilder;

        // Add related services
        services.AddSingleton(typeof(IDeepHealthCheckConfigService<>), typeof(DeepHealthCheckConfigService<>));

        // Get all IHealthCheck implements
        var heathChecks = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IHealthCheck).IsAssignableFrom(type)
                           && type is { IsClass: true, IsAbstract: false })
            .ToList();

        // Register HealthChecks
        foreach (var healtCheckConfig in rootConfig!.HealthChecks!)
        {
            if (heathChecks.All(type => type.Name != healtCheckConfig.HealthCheckName)) continue;

            // Dependency Injection
            switch (healtCheckConfig.ServiceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.TryAddSingleton(typeof(IHealthCheck),
                        heathChecks.First(type => type.Name == healtCheckConfig.HealthCheckName));
                    break;
                case ServiceLifetime.Scoped:
                    services.TryAddScoped(typeof(IHealthCheck),
                        heathChecks.First(type => type.Name == healtCheckConfig.HealthCheckName));
                    break;
                case ServiceLifetime.Transient:
                    services.TryAddTransient(typeof(IHealthCheck),
                        heathChecks.First(type => type.Name == healtCheckConfig.HealthCheckName));
                    break;
                default:
                    continue;
            }

            healthCheckBuilder.Add(new HealthCheckRegistration(
                healtCheckConfig.ServiceName,
                serviceProvider =>
                {
                    var healthCheck = serviceProvider.GetServices<IHealthCheck>().First(s =>
                        s.GetType().Name.Equals(healtCheckConfig.HealthCheckName,
                            StringComparison.CurrentCultureIgnoreCase));
                    return healthCheck;
                },
                healtCheckConfig.FailureStatus,
                healtCheckConfig.Tags,
                healtCheckConfig.TimeoutInSeconds > 0
                    ? TimeSpan.FromSeconds(Convert.ToDouble(healtCheckConfig.TimeoutInSeconds))
                    : null
            ));
        }

        // Add LogSeverityHealthCheck dependencies
        if (IsEnabledLogSeverity(rootConfig!))
        {
            services.AddMemoryCache();
            services.AddSingleton<ILogSeverityLogger, LogSeverityLogger>();
            services.AddSingleton<ILogSeverityLoggerProvider, LogSeverityLoggerProvider>();
            healthCheckBuilder.BindContext<LogSeverityHealthCheck, LogSeverityContext>();
        }

        return healthCheckBuilder;
    }

    public static IHealthChecksBuilder BindContext<THealthCheck, TContext>(this IHealthChecksBuilder builder)
        where THealthCheck : class, IHealthCheck
        where TContext : class
    {
        ConfigurationManager.SetContextOption<THealthCheck, TContext>();
        return builder;
    }

    /// <summary>
    /// Configures the application to use dynamic health checks middleware.
    /// This method sets up a custom middleware that responds to requests at the specified path with health check status.
    /// It also integrates a logger provider for health check logging based on the provided configuration.
    /// </summary>
    /// <param name="app">The application builder used to configure the HTTP request pipeline.</param>
    /// <param name="path">The path where the health check requests will be handled. Defaults to <see cref="Constants.DEFAULT_HEALTH_CHECK_PATH"/>.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> instance for further configuration.</returns>
    public static IApplicationBuilder UseDeepHealthCheck(this IApplicationBuilder app,
        string path = Constants.DEFAULT_HEALTH_CHECK_PATH)
    {
        var healthCheckConfig = ConfigurationManager.Get(app.ApplicationServices.GetService<IConfiguration>()!);

        if (IsEnabledLogSeverity(healthCheckConfig!))
        {
            var provider = app.ApplicationServices.GetRequiredService<ILogSeverityLoggerProvider>();
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(provider);
        }

        // Use custom middleware for health checks
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                var healthCheckService = context.RequestServices.GetRequiredService<HealthCheckService>();
                var report = await healthCheckService.CheckHealthAsync();
                var statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(report.Status.ToString());
            }
            else
            {
                await next();
            }
        });

        return app;
    }

    private static bool IsDisabled(DeepHealthCheckRoot? healthCheckConfig)
    {
        return healthCheckConfig == null ||
               healthCheckConfig.Disabled ||
               healthCheckConfig.HealthChecks is not { Count: > 0 };
    }

    private static bool IsEnabledLogSeverity(DeepHealthCheckRoot? healthCheckConfig)
    {
        return !IsDisabled(healthCheckConfig) &&
               healthCheckConfig!.HealthChecks!
                   .Any(d => d.HealthCheckName == "LogSeverityHealthCheck");
    }
}