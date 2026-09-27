using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Sites;

/// <summary>
/// POST /api/dcim/sites/ request body.
/// </summary>
public sealed record CreateSiteDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
