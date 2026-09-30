using System.Globalization;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Framework.API.Configuration;
using CsharpTestAutomation.Framework.API.Interceptors;
using NLog;
using NLog.Config;
using NLog.Targets;
using RestSharp;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CsharpTestAutomation.Framework.Test.Tests.Api;

[TestFixture]
[NonParallelizable]
[Category("ApiLoggingInterceptorTests")]
public class ApiLoggingInterceptorTests
{
    private const string ServiceName = "wiremock";
    private const string Separator = "|";
    private const string InterceptorLoggerName = "CsharpTestAutomation.Framework.API.Interceptors.ApiLoggingInterceptor";

    private static readonly int[] s_delaysInMs = [100, 1_100, 2_100];

    private WireMockServer _server = null!;
    private MemoryTarget _logTarget = null!;
    private RestClientFactory _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _server = WireMockServer.Start();

        foreach (var delay in s_delaysInMs)
        {
            _server
                .Given(Request.Create().WithPath($"/delay/{delay}/*").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200).WithDelay(TimeSpan.FromMilliseconds(delay)));
        }

        _server
            .Given(Request.Create().WithPath("/multibyte/*").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain; charset=utf-8")
                .WithBody(new string('é', 9)));

        _server
            .Given(Request.Create().WithPath("/ok/*").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200));

        _logTarget = new MemoryTarget($"api-interceptor-{Guid.NewGuid():N}") {
            Layout = $"${{scopeproperty:{ApiLoggingInterceptor.TestNameScopeProperty}}}{Separator}${{message}}"
        };

        LogManager.Configuration ??= new LoggingConfiguration();
        LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, _logTarget, InterceptorLoggerName);
        LogManager.ReconfigExistingLoggers();

        _factory = new RestClientFactory(new ApiSettings {
            Services = new Dictionary<string, ApiServiceSettings> {
                [ServiceName] = new() { BaseUrl = _server.Url!, TimeoutSeconds = 30 }
            },
            Logging = new ApiLoggingSettings { AttachToAllure = false, LogFullDetail = true, MaxBodySizeBytes = 10 }
        });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        LogManager.Configuration?.RemoveTarget(_logTarget.Name);
        LogManager.ReconfigExistingLoggers();
        _logTarget.Dispose();

        _server.Stop();
        _server.Dispose();
    }

    [Test]
    public async Task AfterRequest_ParallelCallsWithDifferentDelays_LogsElapsedTimeOfEachCall()
    {
        using IRestClient client = _factory.Create(ServiceName);
        var runId = Guid.NewGuid().ToString("N");
        await client.ExecuteAsync(new RestRequest($"ok/{runId}-warmup"));

        Task<RestResponse>[] calls = [.. Enumerable.Range(0, 9)
            .Select(i => client.ExecuteAsync(new RestRequest($"delay/{s_delaysInMs[i % s_delaysInMs.Length]}/{runId}-{i}")))];
        await Task.WhenAll(calls);

        for (var i = 0; i < calls.Length; i++)
        {
            var delay = s_delaysInMs[i % s_delaysInMs.Length];
            var elapsedMs = ReadElapsedMs($"delay/{delay}/{runId}-{i}");

            elapsedMs.Should().BeInRange(delay - 50, delay + 999, $"call {i} was delayed by {delay}ms on the server");
        }
    }

    [Test]
    public async Task AfterRequest_CallFromTest_LogsTestNameScopeProperty()
    {
        using IRestClient client = _factory.Create(ServiceName);
        var resource = $"ok/{Guid.NewGuid():N}";

        await client.ExecuteAsync(new RestRequest(resource));

        var summary = FindSummaryLine(resource);
        summary.Should().StartWith(TestContext.CurrentContext.Test.FullName + Separator);
    }

    [Test]
    public async Task AfterRequest_MultiByteBodyOverLimit_TruncatesByUtf8Bytes()
    {
        using IRestClient client = _factory.Create(ServiceName);
        var resource = $"multibyte/{Guid.NewGuid():N}";

        await client.ExecuteAsync(new RestRequest(resource));

        var detail = _logTarget.Logs.Single(line => line.Contains("=== API RESPONSE ===") && line.Contains(resource));
        detail.Should().Contain(new string('é', 5) + " [TRUNCATED]");
        detail.Should().NotContain(new string('é', 6));
    }

    private string FindSummaryLine(string resource) =>
        _logTarget.Logs.Single(line => line.Contains($"[API] GET {resource} "));

    private int ReadElapsedMs(string resource)
    {
        Match match = Regex.Match(FindSummaryLine(resource), @"\| (\d+)ms$");
        match.Success.Should().BeTrue($"the summary line for '{resource}' should contain the elapsed time");
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }
}
