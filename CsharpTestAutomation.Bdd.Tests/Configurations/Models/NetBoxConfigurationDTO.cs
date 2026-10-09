namespace CsharpTestAutomation.Bdd.Tests.Configurations.Models;

public sealed class NetBoxConfigurationDTO
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? ApiToken { get; set; }
}
