using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.Manufacturers;

/// <summary>
/// POST /api/dcim/manufacturers/ request body.
/// </summary>
public sealed record CreateManufacturerDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}
