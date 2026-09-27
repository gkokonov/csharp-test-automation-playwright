using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;

/// <summary>
/// GET/POST /api/dcim/manufacturers/ response shape (list, detail, and create share one schema).
/// </summary>
public sealed record ManufacturerDto
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
}
