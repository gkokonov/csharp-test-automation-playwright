using System.Text.Json.Serialization;
using CsharpTestAutomation.Tests.Api.Dtos.Common;

namespace CsharpTestAutomation.Tests.Api.Dtos.Ipam;

/// <summary>
/// GET /api/ipam/ip-addresses/{id}/ response. Also used as the create/update response and as the
/// item shape of GET /api/ipam/ip-addresses/ (NetBox uses the same schema for detail and list item).
/// </summary>
public sealed record IpAddressDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public StatusFieldDto Status { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
