using System.Collections.Concurrent;
using NLog;

namespace CsharpTestAutomation.Tests.Tests;

/// <summary>
/// Thread-safe service container for test automation that supports named registrations.
/// </summary>
public class TestContainer
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    private readonly ConcurrentDictionary<(Type, string), object> _services = new();

    /// <summary>
    /// Register a service instance with the container (default, unnamed).
    /// </summary>
    public void Register<T>(T service) where T : class => Register(service, null);

    /// <summary>
    /// Register a service instance with the container under a specific name.
    /// </summary>
    public void Register<T>(T service, string? name) where T : class
    {
        if (service == null)
        {
            s_log.Warn($"Attempted to register null service of type {typeof(T).Name} (name: {name})");
            return;
        }

        _services[(typeof(T), name ?? string.Empty)] = service;
    }

    /// <summary>
    /// Get a service from the container (default, unnamed).
    /// </summary>
    public T? Get<T>() where T : class => Get<T>(null);

    /// <summary>
    /// Get a named service from the container.
    /// </summary>
    public T? Get<T>(string? name) where T : class
    {
        if (_services.TryGetValue((typeof(T), name ?? string.Empty), out var service))
        {
            return (T)service;
        }

        s_log.Warn($"Service of type {typeof(T).Name} (name: {name}) not found in container");
        return null;
    }

    /// <summary>
    /// Check if a service is registered (default, unnamed).
    /// </summary>
    public bool HasService<T>() where T : class => HasService<T>(null);

    /// <summary>
    /// Check if a named service is registered.
    /// </summary>
    public bool HasService<T>(string? name) where T : class
        => _services.ContainsKey((typeof(T), name ?? string.Empty));

    /// <summary>
    /// Clear all services from the container.
    /// </summary>
    public void Clear() => _services.Clear();

    /// <summary>
    /// Try to dispose all disposable services in the container.
    /// </summary>
    public async Task DisposeServicesAsync()
    {
        foreach (var service in _services.Values)
        {
            if (service is IAsyncDisposable asyncDisposable)
            {
                try
                { await asyncDisposable.DisposeAsync(); }
                catch (Exception ex) { s_log.Error(ex, $"Error async disposing service of type {service.GetType().Name}"); }
            }
            else if (service is IDisposable disposable)
            {
                try
                { disposable.Dispose(); }
                catch (Exception ex) { s_log.Error(ex, $"Error disposing service of type {service.GetType().Name}"); }
            }
        }
    }
}
