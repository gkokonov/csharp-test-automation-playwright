using AwesomeAssertions;
using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Framework.API.Configuration;

namespace CsharpTestAutomation.Framework.Test.Tests.Api;

[TestFixture]
[Category("RestClientFactoryTests")]
public class RestClientFactoryTests
{
    private const string ServiceName = "demo";

    private static RestClientFactory CreateFactory(ApiSettings settings) => new(settings);

    private static ApiSettings ValidSettings(Action<ApiServiceSettings>? configureService = null)
    {
        var service = new ApiServiceSettings { BaseUrl = "https://api.example.com", TimeoutSeconds = 30 };
        configureService?.Invoke(service);

        return new ApiSettings {
            Services = new Dictionary<string, ApiServiceSettings> { [ServiceName] = service },
            Logging = new ApiLoggingSettings { MaxBodySizeBytes = 10_000 }
        };
    }

    [Test]
    public void Verify_ClientCreatedWithConfiguredBaseUrl_When_SettingsAreValid()
    {
        RestClientFactory factory = CreateFactory(ValidSettings());

        using RestSharp.IRestClient client = factory.Create(ServiceName);

        client.Options.BaseUrl.Should().Be(new Uri("https://api.example.com"));
    }

    [Test]
    public void Verify_ArgumentExceptionThrown_When_ServiceNameIsUnknown()
    {
        RestClientFactory factory = CreateFactory(ValidSettings());

        Action act = () => factory.Create("missing");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing*");
    }

    [Test]
    public void Verify_ArgumentExceptionThrown_When_BaseUrlIsEmpty()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.BaseUrl = string.Empty));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{ServiceName}*BaseUrl*");
    }

    [Test]
    public void Verify_UriFormatExceptionThrown_When_BaseUrlIsRelative()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.BaseUrl = "not-a-valid-uri"));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*absolute URI*");
    }

    [Test]
    public void Verify_ArgumentOutOfRangeExceptionThrown_When_TimeoutIsNotPositive()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.TimeoutSeconds = 0));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TimeoutSeconds*");
    }

    [Test]
    public void Verify_ArgumentOutOfRangeExceptionThrown_When_MaxBodySizeIsNotPositive()
    {
        ApiSettings settings = ValidSettings();
        settings.Logging.MaxBodySizeBytes = 0;
        RestClientFactory factory = CreateFactory(settings);

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxBodySizeBytes*");
    }
}
