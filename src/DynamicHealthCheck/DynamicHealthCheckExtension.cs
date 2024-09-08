using DynamicHealthCheck.Models;
using DynamicHealthCheck.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck;

public static class DynamicHealthCheckExtension
{
    /// <summary>
    ///     Registers dynamic health checks based on the provided configuration.
    ///     This method scans the application's assemblies for implementations of <see cref="IHealthCheck" /> and registers
    ///     them if configured well.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">The application configuration used to retrieve health check settings.</param>
    /// <param name="configSectionName">
    ///     The name of the configuration section containing health check settings. Defaults to
    ///     <see cref="Constants.DEFAULT_CONFIG_SECTION_NAME" />.
    /// </param>
    /// <returns>An <see cref="IHealthChecksBuilder" /> instance for further health check configuration.</returns>
    public static IHealthChecksBuilder AddDynamicHealthCheck(this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = Constants.DEFAULT_CONFIG_SECTION_NAME)
    {
        ConfigurationManager.ConfigSection = configSectionName;

        var healthChecksBuilder = services.AddHealthChecks();

        var rootConfig = ConfigurationManager.Get(configuration);

        // Add normal HealthChecks only if disabled.
        if (IsDisabled(rootConfig)) return healthChecksBuilder;

        // Add dynamic health check config service
        services.AddSingleton(typeof(IDynamicHealthCheckConfigService<>), typeof(DynamicHealthCheckConfigService<>));

        // Get all IHealthCheck implements
        var heathChecks = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IHealthCheck).IsAssignableFrom(type)
                           && type is { IsClass: true, IsAbstract: false })
            .ToList();

        // Parse HealthChecks configurations
        foreach (var healthCheckConfig in rootConfig!.HealthChecks!)
        {
            if (heathChecks.All(type => type.Name != healthCheckConfig.HealthCheckName)) continue;

            var healthCheckType = heathChecks.First(type => type.Name == healthCheckConfig.HealthCheckName);

            // DI health check
            switch (healthCheckConfig.ServiceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.TryAddSingleton(healthCheckType);
                    break;
                case ServiceLifetime.Scoped:
                    services.TryAddScoped(healthCheckType);
                    break;
                case ServiceLifetime.Transient:
                    services.TryAddTransient(healthCheckType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Register health check
            healthChecksBuilder.Add(new HealthCheckRegistration(
                healthCheckConfig.ServiceName,
                serviceProvider =>
                    HealthCheckFactory.CreateHealthCheck(serviceProvider, healthCheckType, healthCheckConfig.ServiceLifetime),
                healthCheckConfig.FailureStatus,
                healthCheckConfig.Tags,
                healthCheckConfig.TimeoutInSeconds > 0
                    ? TimeSpan.FromSeconds(Convert.ToDouble(healthCheckConfig.TimeoutInSeconds))
                    : null
            ));
        }

        return healthChecksBuilder;
    }

    /// <summary>
    ///     Binds a custom health check to its corresponding context model, enabling the health check instance to easily access its configuration context through dependency injection via <see cref="IDynamicHealthCheckConfigService{THealthCheck}" />.
    /// </summary>
    /// <typeparam name="THealthCheck">Custom health check class.</typeparam>
    /// <typeparam name="TContext">Custom health check context class.</typeparam>
    /// <returns>An <see cref="IHealthChecksBuilder" /> instance for further health check configuration.</returns>
    public static IHealthChecksBuilder BindHealthCheckContext<THealthCheck, TContext>(this IHealthChecksBuilder builder)
        where THealthCheck : class, IHealthCheck
        where TContext : class
    {
        ConfigurationManager.SetHealthCheckContext<THealthCheck, TContext>();
        return builder;
    }

    /// <summary>
    ///     Configures the application to use dynamic health checks middleware.
    ///     This method sets up a custom middleware that responds to requests at the specified path with health check status.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="path">
    ///     The path where the health check requests will be handled. Defaults to
    ///     <see cref="Constants.DEFAULT_HEALTH_CHECK_PATH" />.
    /// </param>
    /// <returns>The updated <see cref="IApplicationBuilder" /> instance for further configuration.</returns>
    public static IApplicationBuilder UseDynamicHealthCheck(this IApplicationBuilder app,
        string path = Constants.DEFAULT_HEALTH_CHECK_PATH)
    {
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
        
        // Apply all dependencies
        MiddlewareManager.ApplyMiddlewares(app);
        
        return app;
    }

    private static bool IsDisabled(DynamicHealthCheckRoot? healthCheckConfig)
    {
        return healthCheckConfig == null ||
               healthCheckConfig.Disabled ||
               healthCheckConfig.HealthChecks is not { Count: > 0 };
    }

}