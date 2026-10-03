using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Extensions;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Database;
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
        WriteAllureEnvironment();
        await CheckDependenciesAsync();

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

    private static async Task CheckDependenciesAsync()
    {
        if (string.IsNullOrWhiteSpace(s_configuration.NetBox.ApiToken)
            && (string.IsNullOrWhiteSpace(s_configuration.NetBox.Username)
                || string.IsNullOrWhiteSpace(s_configuration.NetBox.Password)))
        {
            throw new InvalidOperationException(
                "Configure NetBox.ApiToken or both NetBox.Username and NetBox.Password.");
        }

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        await DependencyAvailabilityChecker.EnsureAllAvailableAsync(httpClient,
        [
            ("NetBox UI", s_configuration.NetBox.BaseUrl),
            ("NetBox API", s_configuration.NetBox.ApiBaseUrl)
        ]);

        try
        {
            PostgreSqlConnectionPool.Instance.OpenConnections();
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "PostgreSQL dependency check failed.");
            throw;
        }
    }

    private static void WriteAllureEnvironment()
    {
        AllureEnvironmentWriter.Write(
        [
            new("Environment", Environment.GetEnvironmentVariable("Environment") ?? "local"),
            new("NetBox.BaseUrl", s_configuration.NetBox.BaseUrl),
            new("NetBox.ApiBaseUrl", s_configuration.NetBox.ApiBaseUrl),
            new("Browser", s_configuration.BrowserType),
            new("Headless", s_configuration.HeadlessMode.ToString()),
            new("BuildNumber", s_configuration.BuildNumber),
            new("OS", RuntimeInformation.OSDescription),
            new("DotNet", Environment.Version.ToString()),
        ]);
    }

    /// <summary>
    /// Logs the full configuration object graph (nested DTOs and models) for the current environment.
    /// Values of sensitive keys are redacted.
    /// </summary>
    private static void LogAppsettingsValues()
    {
        s_log.Debug("Log appsettings values for execution environment: " + (Environment.GetEnvironmentVariable("Environment") ?? "local"));

        try
        {
            JsonNode? node = JsonSerializer.SerializeToNode(s_configuration, s_configuration.GetType(), JsonExtensions.DefaultOptions);
            s_log.Debug(node?.ToJsonString(JsonExtensions.DefaultOptions) ?? "null");
        }
        catch (Exception ex)
        {
            s_log.Warn(ex, "Unable to log appsettings values.");
        }
    }
}
