using System.Text;
using System.Text.Json;
using CsharpTestAutomation.Tests.Configurations.Models;

namespace CsharpTestAutomation.Tests.Authentication;

public sealed class NetBoxAuthClient(HttpClient httpClient, NetBoxConfigurationDTO configuration)
{
    private const string TokenProvisioningPath = "users/tokens/provision/";

    public async Task<string> ProvisionTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(configuration.ApiBaseUrl, UriKind.Absolute, out Uri? apiBaseUri))
        {
            throw new InvalidOperationException("NetBox.ApiBaseUrl must be an absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(configuration.Username) || string.IsNullOrWhiteSpace(configuration.Password))
        {
            throw new InvalidOperationException("NetBox.Username and NetBox.Password must be configured to provision an API token.");
        }

        Uri endpoint = new(apiBaseUri, TokenProvisioningPath);

        // NetBox's dev server rejects a chunked-transfer body (PostAsJsonAsync's streamed JsonContent);
        // a buffered StringContent sends an explicit Content-Length instead.
        // "version": 1 requests a classic single-secret token; NetBox 4.x defaults to v2, whose
        // split key/secret pair requires the "Bearer <key>.<token>" scheme instead of this client's
        // "Authorization: Token <value>" (see NetBoxTokenAuthenticator).
        var requestBody = JsonSerializer.Serialize(new { username = configuration.Username, password = configuration.Password, version = 1 });
        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync(endpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"NetBox token provisioning failed with HTTP {(int)response.StatusCode} ({response.StatusCode}).");
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument payload = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        if (!payload.RootElement.TryGetProperty("token", out JsonElement tokenValue)
            || tokenValue.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(tokenValue.GetString()))
        {
            throw new InvalidOperationException("NetBox token provisioning response did not contain a token value.");
        }

        return tokenValue.GetString()!;
    }
}
