using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Devices;

/// <summary>
/// PATCH /api/dcim/devices/{id}/ request body.
/// </summary>
public sealed record UpdateDeviceDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
