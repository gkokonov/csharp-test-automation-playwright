using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;

/// <summary>
/// POST /api/ipam/prefixes/ request body.
/// </summary>
public sealed record CreatePrefixDto
{
    [JsonPropertyName("prefix")]
    public string Prefix { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
