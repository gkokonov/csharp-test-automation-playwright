#nullable enable

using System.Net;
using CsharpTestAutomation.Framework.API.Configuration;
using CsharpTestAutomation.Framework.API.Interceptors;
using CsharpTestAutomation.Framework.API.Redaction;
using CsharpTestAutomation.Framework.Common.Extensions;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.Json;

namespace CsharpTestAutomation.Framework.API.Clients;

/// <summary>
/// Default <see cref="IRestClientFactory"/> implementation. Resolves per-service settings from
/// <see cref="ApiSettings"/>, builds and wires the shared <see cref="ApiLoggingInterceptor"/>
/// (with its <see cref="ApiLogSanitizer"/>) internally, and configures the built-in
/// System.Text.Json serializer with <see cref="JsonExtensions.DefaultOptions"/>.
/// </summary>
/// <param name="settings">API settings supplying named service and logging configuration.</param>
/// <param name="proxy">Optional web proxy applied to every created client; <see langword="null"/> to use no proxy.</param>
public sealed class RestClientFactory(ApiSettings settings, IWebProxy? proxy = null) : IRestClientFactory
{
    private readonly ApiLoggingInterceptor _interceptor =
        new(new ApiLogSanitizer(settings.Logging), settings.Logging);

    /// <inheritdoc />
    public IRestClient Create(string serviceName, IAuthenticator? authenticator = null)
    {
        if (!settings.Services.TryGetValue(serviceName, out ApiServiceSettings? serviceSettings))
        {
            throw new InvalidOperationException(
                $"No API service named '{serviceName}' is configured under Api.Services in appsettings.json.");
        }

        serviceSettings.Validate(serviceName);
        settings.Logging.Validate();

        var options = new RestClientOptions(serviceSettings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(serviceSettings.TimeoutSeconds),
            Interceptors = [_interceptor],
            Authenticator = authenticator,
            ThrowOnAnyError = false,
            FailOnDeserializationError = true,
            Proxy = proxy
        };

        return new RestClient(
            options,
            configureSerialization: s => s.UseSystemTextJson(JsonExtensions.DefaultOptions));
    }
}
