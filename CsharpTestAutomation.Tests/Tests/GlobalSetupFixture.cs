using System.Reflection;
using System.Text.Json;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Extensions;
using CsharpTestAutomation.Tests.Authentication;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Database;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Tests.Tests;

[SetUpFixture]
public class GlobalSetupFixture
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    private static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        s_log.Debug("Global one-time setup starting...");

        LogAppsettingsValues();

        await BootstrapAuthenticationAsync();

        s_log.Debug("Global one-time setup complete.");
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        s_log.Debug("Global one-time teardown starting...");

        try
        {
            // Close the shared PostgreSQL connection pool once, after every fixture has finished.
            PostgreSqlConnectionPool.Instance.CloseConnections();
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "Error during global teardown.");
        }

        s_log.Debug("Global one-time teardown complete");
        await Task.CompletedTask;
    }

    private static async Task BootstrapAuthenticationAsync()
    {
        if (s_configuration.Ui is null)
        {
            s_log.Debug("No UI configuration present; skipping authentication bootstrap.");
            return;
        }

        try
        {
            await UiAuthenticationBootstrapper.BootstrapAllAsync(s_configuration.Ui);
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "UI authentication bootstrap failed.");
            throw;
        }
    }

    /// <summary>
    /// Logs appsettings values for the current environment.
    /// </summary>
    private static void LogAppsettingsValues()
    {
        s_log.Debug("Log appsettings values for execution environment: " + (Environment.GetEnvironmentVariable("Environment") ?? "local"));

        // Create dictionary to hold property values
        var configValues = new Dictionary<string, string>();

        // Get all instance properties from ExtendedConfiguration and its base class CoreConfiguration
        Type type = typeof(ExtendedConfiguration);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (PropertyInfo pi in properties)
        {
            try
            {
                // Get the property value from s_configuration instance and convert to string if
                // not null
                var value = pi.GetValue(s_configuration);

                // Handle ViewportSize specially
                if (pi.Name == nameof(CoreConfiguration.ViewportSize) && value is ViewportSize viewportSize)
                {
                    configValues[pi.Name] = $"{viewportSize.Width}x{viewportSize.Height}";
                }
                else
                {
                    configValues[pi.Name] = value?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                configValues[pi.Name] = $"Error retrieving value: {ex.Message}";
            }
        }

        var output = JsonSerializer.Serialize(configValues, JsonExtensions.DefaultOptions);
        s_log.Debug(output);
    }
}
