namespace CsharpTestAutomation.Tests.Configurations.Models;

public class EntraIdConfigurationDTO
{
    public string? EntraIdClientId { get; set; }
    public string? EntraIdClientSecret { get; set; }
    public string? EntraIdGrantType { get; set; }
    public string? EntraIdTenantId { get; set; }

    /// <summary>
    /// Named scopes map. Key = logical name (e.g. "InvitationApi"), Value = full scope URI.
    /// </summary>
    public Dictionary<string, string> EntraIdScopes { get; set; } = [];
}
