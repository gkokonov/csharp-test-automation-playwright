using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Ipam;

/// <summary>
/// PATCH /api/ipam/prefixes/{id}/ request body.
/// </summary>
public sealed record UpdatePrefixDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
