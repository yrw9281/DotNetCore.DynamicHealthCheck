using System.Reflection;
using DynamicHealthCheck.Abstractions;
using DynamicHealthCheck.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicHealthCheck.Configuration;

internal sealed class DynamicHealthCheckConfigurationProvider : IDynamicHealthCheckConfigurationProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _configSectionName;
    private readonly IHealthCheckContextRegistry _contextRegistry;
    private readonly object _syncRoot = new();
    private bool _contextsInitialized;

    public DynamicHealthCheckConfigurationProvider(
        IConfiguration configuration,
        string configSectionName,
        IHealthCheckContextRegistry contextRegistry)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _configSectionName = string.IsNullOrWhiteSpace(configSectionName)
            ? throw new ArgumentException("Configuration section name cannot be null or whitespace.", nameof(configSectionName))
            : configSectionName;
        _contextRegistry = contextRegistry ?? throw new ArgumentNullException(nameof(contextRegistry));
    }

    public DynamicHealthCheckRoot? GetRoot()
    {
        return GetRootSection().Get<DynamicHealthCheckRoot>();
    }

    public DynamicHealthCheckConfig GetHealthCheckConfig<THealthCheck>(string serviceName)
        where THealthCheck : class, IHealthCheck
    {
        var healthCheckName = typeof(THealthCheck).Name;

        var configs = GetHealthCheckConfigsByHealthCheckName(healthCheckName);
        var match = configs?.FirstOrDefault(config =>
            string.Equals(config.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

        return match ?? throw new ArgumentException(
            $"Health check '{healthCheckName}' with service name '{serviceName}' is not configured correctly.");
    }

    public TContext GetHealthCheckContext<THealthCheck, TContext>(string serviceName)
        where THealthCheck : class, IHealthCheck
        where TContext : class
    {
        EnsureContextBindings();

        if (!_contextRegistry.TryGet(typeof(THealthCheck), out var contextType) || contextType != typeof(TContext))
        {
            throw new ArgumentException(
                $"Health check '{typeof(THealthCheck).Name}' has not been bound to context '{typeof(TContext).Name}'.");
        }

        var section = FindHealthCheckSection(typeof(THealthCheck).Name, serviceName);
        var context = section?
            .GetSection(Constants.CONFIG_SECTION_PROPERTY_NAME_CONTEXT)
            .Get<TContext>();

        return context ?? throw new ArgumentException(
            $"Unable to resolve context '{typeof(TContext).Name}' for health check '{typeof(THealthCheck).Name}'.");
    }

    public IEnumerable<TContext> GetHealthCheckContexts<THealthCheck, TContext>()
        where THealthCheck : class, IHealthCheck
        where TContext : class
    {
        EnsureContextBindings();

        if (!_contextRegistry.TryGet(typeof(THealthCheck), out var contextType) || contextType != typeof(TContext))
        {
            throw new ArgumentException(
                $"Health check '{typeof(THealthCheck).Name}' has not been bound to context '{typeof(TContext).Name}'.");
        }

        var sections = FindHealthCheckSections(typeof(THealthCheck).Name);
        var result = new List<TContext>();

        foreach (var section in sections)
        {
            var context = section
                .GetSection(Constants.CONFIG_SECTION_PROPERTY_NAME_CONTEXT)
                .Get<TContext>();

            if (context != null)
            {
                result.Add(context);
            }
        }

        if (result.Count == 0)
        {
            throw new ArgumentException(
                $"No context configuration found for health check '{typeof(THealthCheck).Name}'.");
        }

        return result;
    }

    private IConfigurationSection GetRootSection()
    {
        return _configuration.GetSection(_configSectionName);
    }

    private IConfigurationSection GetHealthChecksSection()
    {
        return GetRootSection().GetSection(Constants.CONFIG_SECTION_PROPERTY_NAME_HEALTH_CHECKS);
    }

    private IEnumerable<DynamicHealthCheckConfig>? GetHealthCheckConfigsByHealthCheckName(string healthCheckName)
    {
        return GetHealthChecksSection()
            .Get<List<DynamicHealthCheckConfig>>()?
            .Where(config => string.Equals(config.HealthCheckName, healthCheckName, StringComparison.OrdinalIgnoreCase));
    }

    private IConfigurationSection? FindHealthCheckSection(string healthCheckName, string serviceName)
    {
        return GetHealthCheckSections()
            .FirstOrDefault(section => Matches(section, healthCheckName, serviceName));
    }

    private IEnumerable<IConfigurationSection> FindHealthCheckSections(string healthCheckName)
    {
        return GetHealthCheckSections()
            .Where(section => Matches(section, healthCheckName, serviceName: null));
    }

    private IEnumerable<IConfigurationSection> GetHealthCheckSections()
    {
        return GetHealthChecksSection().GetChildren();
    }

    private static bool Matches(IConfigurationSection section, string healthCheckName, string? serviceName)
    {
        var configuredHealthCheckName = section.GetValue<string>(nameof(DynamicHealthCheckConfig.HealthCheckName));
        var configuredServiceName = section.GetValue<string>(nameof(DynamicHealthCheckConfig.ServiceName));

        if (!string.Equals(configuredHealthCheckName, healthCheckName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(serviceName))
        {
            return true;
        }

        return string.Equals(configuredServiceName, serviceName, StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureContextBindings()
    {
        if (_contextsInitialized)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_contextsInitialized)
            {
                return;
            }

            foreach (var (healthCheckType, contextType) in DiscoverHealthCheckContextMappings())
            {
                if (healthCheckType is null || contextType is null)
                {
                    continue;
                }

                if (!typeof(IHealthCheck).IsAssignableFrom(healthCheckType))
                {
                    continue;
                }

                if (contextType.IsAbstract || contextType.IsInterface)
                {
                    continue;
                }

                _contextRegistry.TryAdd(healthCheckType, contextType);
            }

            _contextsInitialized = true;
        }
    }

    private static IEnumerable<(Type? HealthCheckType, Type? ContextType)> DiscoverHealthCheckContextMappings()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] candidateTypes;

            try
            {
                candidateTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                candidateTypes = ex.Types.OfType<Type>().ToArray();
            }

            foreach (var candidate in candidateTypes.Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var implementedInterface in candidate.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    var genericDefinition = implementedInterface.GetGenericTypeDefinition();
                    if (genericDefinition != typeof(IHealthCheckContext<>))
                    {
                        continue;
                    }

                    yield return (implementedInterface.GenericTypeArguments.FirstOrDefault(), candidate);
                }
            }
        }
    }
}
