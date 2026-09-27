using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Ipam;

/// <summary>
/// POST /api/ipam/ip-addresses/ request body.
/// </summary>
public sealed record CreateIpAddressDto
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
