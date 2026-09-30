using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace CsharpTestAutomation.Framework.Common;

/// <summary>
/// Generic singleton configuration manager that loads and exposes strongly-typed settings.
/// <para>
/// <see cref="Instance"/> layers sources in this order, later sources overriding earlier ones:
/// <c>appsettings.json</c>, <c>appsettings.{Environment}.json</c> (when the <c>Environment</c>
/// environment variable is set), the optional <c>appsettings.local.json</c>, and environment variables.
/// <c>appsettings.json</c> is required unless an environment file is used; the environment file is required
/// when <c>Environment</c> is set. Use <see cref="InstanceWithConfigName"/> when a named configuration
/// file is required, for example for isolated service configurations or parallel test fixture setups.
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
    private const string EnvironmentVariableName = "Environment";

    private static readonly Lazy<AppConfiguration<TSettingsModel>> s_instance = new(() => new AppConfiguration<TSettingsModel>());
    private static readonly ConcurrentDictionary<string, AppConfiguration<TSettingsModel>> s_namedInstances = new();

    private readonly IConfiguration _configuration;

    private AppConfiguration()
    {
        _configuration = BuildDefaultConfiguration(
            AppDomain.CurrentDomain.BaseDirectory,
            Environment.GetEnvironmentVariable(EnvironmentVariableName));

        SetConfigurationValue();
    }

    internal static IConfiguration BuildDefaultConfiguration(string basePath, string? environmentName)
    {
        var hasEnvironment = !string.IsNullOrWhiteSpace(environmentName);

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: hasEnvironment);

        if (hasEnvironment)
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json");
        }

        return builder
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
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
