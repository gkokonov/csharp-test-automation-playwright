using System.Text.Json.Serialization;
using CsharpTestAutomation.Tests.Api.Dtos.Common;

namespace CsharpTestAutomation.Tests.Api.Dtos.Ipam;

/// <summary>
/// GET /api/ipam/prefixes/{id}/ response. Also used as the create/update response and as the
/// item shape of GET /api/ipam/prefixes/ (NetBox uses the same schema for detail and list item).
/// </summary>
public sealed record PrefixDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    [JsonPropertyName("prefix")]
    public string Prefix { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public StatusFieldDto Status { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
