using System.Text.Json.Serialization;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Common;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;

/// <summary>
/// GET /api/dcim/sites/{id}/ response. Also used as the create/update response and as the item
/// shape of GET /api/dcim/sites/ (NetBox uses the same schema for detail and list item).
/// </summary>
public sealed record SiteDetailDto
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

    [JsonPropertyName("status")]
    public StatusFieldDto Status { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
