using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;

/// <summary>
/// GET/POST /api/dcim/device-roles/ response shape (list, detail, and create share one schema).
/// </summary>
public sealed record DeviceRoleResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

