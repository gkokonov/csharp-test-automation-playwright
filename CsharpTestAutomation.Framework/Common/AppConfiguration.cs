using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace CsharpTestAutomation.Framework.Common;

/// <summary>
/// Generic singleton configuration manager that loads and exposes strongly-typed settings
/// from <c>appsettings.json</c> or an environment-specific override
/// (<c>appsettings.{Environment}.json</c>), with optional environment variable merging.
/// <para>
/// Use <see cref="Instance"/> for the default singleton backed by the standard
/// <c>appsettings.json</c> / <c>CURRENT_ENV</c> environment variable convention.
/// Use <see cref="InstanceWithConfigName"/> when a named configuration file is required,
/// e.g. for isolated service configurations or parallel test fixture setups.
/// </para>
/// </summary>
/// <typeparam name="TSettingsModel">
/// The strongly-typed settings model to bind configuration values to.
/// Must be a reference type with a parameterless constructor.
/// Typically a class that extends <see cref="CoreConfiguration"/> (e.g., <c>ExtendedConfiguration</c>).
/// </typeparam>
public class AppConfiguration<TSettingsModel>
    where TSettingsModel : class, new()
{
    private static readonly Lazy<AppConfiguration<TSettingsModel>> s_instance = new(() => new AppConfiguration<TSettingsModel>());
    private static readonly ConcurrentDictionary<string, AppConfiguration<TSettingsModel>> s_namedInstances = new();

    private readonly IConfiguration _configuration;

    private AppConfiguration()
    {
        var currentEnvironment = Environment.GetEnvironmentVariable("Environment")!;
        if (!string.IsNullOrWhiteSpace(currentEnvironment))
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.{currentEnvironment}.json")
                .AddEnvironmentVariables() // this will overwrite keys with the same name and their values
                .Build();
        }
        else
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        SetConfigurationValue();
    }

    private AppConfiguration(string configurationName, bool loadEnvironmentVariables)
    {
        if (!loadEnvironmentVariables)
        {
            _configuration = new ConfigurationBuilder()
                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                 .AddJsonFile($"{configurationName}.json")
                 .Build();
        }
        else
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"{configurationName}.json")
                .AddEnvironmentVariables()
                .Build();
        }

        SetConfigurationValue();
    }

    public static AppConfiguration<TSettingsModel> Instance => s_instance.Value;

    public static AppConfiguration<TSettingsModel> InstanceWithConfigName(string configName, bool loadEnvironmentVariables = false) =>
        s_namedInstances.GetOrAdd($"{configName}_{loadEnvironmentVariables}", _ => new(configName, loadEnvironmentVariables));

    public TSettingsModel Settings { get; private set; } = new();

    private void SetConfigurationValue()
    {
        Settings = _configuration.Get<TSettingsModel>() ??
            throw new ArgumentNullException($"Configuration file could not be loaded correctly from {AppDomain.CurrentDomain.BaseDirectory}.");
    }
}
