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

    private static ApiSettings ValidSettings(Action<ApiServiceSettings> configureService = null)
    {
        var service = new ApiServiceSettings { BaseUrl = "https://api.example.com", TimeoutSeconds = 30 };
        configureService?.Invoke(service);

        return new ApiSettings
        {
            Services = new Dictionary<string, ApiServiceSettings> { [ServiceName] = service },
            Logging = new ApiLoggingSettings { MaxBodySizeBytes = 10_000 }
        };
    }

    [Test]
    public void Create_ValidSettings_ReturnsClientWithConfiguredBaseUrl()
    {
        RestClientFactory factory = CreateFactory(ValidSettings());

        using RestSharp.IRestClient client = factory.Create(ServiceName);

        client.Options.BaseUrl.Should().Be(new Uri("https://api.example.com"));
    }

    [Test]
    public void Create_UnknownServiceName_ThrowsWithServiceName()
    {
        RestClientFactory factory = CreateFactory(ValidSettings());

        Action act = () => factory.Create("missing");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing*");
    }

    [Test]
    public void Create_EmptyBaseUrl_ThrowsWithSettingKey()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.BaseUrl = string.Empty));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{ServiceName}*BaseUrl*");
    }

    [Test]
    public void Create_RelativeBaseUrl_ThrowsInvalidUri()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.BaseUrl = "not-a-valid-uri"));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*absolute URI*");
    }

    [Test]
    public void Create_NonPositiveTimeout_Throws()
    {
        RestClientFactory factory = CreateFactory(ValidSettings(s => s.TimeoutSeconds = 0));

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TimeoutSeconds*");
    }

    [Test]
    public void Create_NonPositiveMaxBodySize_Throws()
    {
        ApiSettings settings = ValidSettings();
        settings.Logging.MaxBodySizeBytes = 0;
        RestClientFactory factory = CreateFactory(settings);

        Action act = () => factory.Create(ServiceName);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxBodySizeBytes*");
    }
}
