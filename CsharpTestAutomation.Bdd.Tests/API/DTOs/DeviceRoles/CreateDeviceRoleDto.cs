using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceRoles;

/// <summary>
/// POST /api/dcim/device-roles/ request body.
/// </summary>
public sealed record CreateDeviceRoleDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}
