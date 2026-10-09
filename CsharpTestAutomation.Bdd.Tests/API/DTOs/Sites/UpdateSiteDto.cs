using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;

/// <summary>
/// PATCH /api/dcim/sites/{id}/ request body.
/// </summary>
public sealed record UpdateSiteDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
