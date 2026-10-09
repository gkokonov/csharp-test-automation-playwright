using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceTypes;

/// <summary>
/// POST /api/dcim/device-types/ request body.
/// </summary>
public sealed record CreateDeviceTypeDto
{
    [JsonPropertyName("manufacturer")]
    public int Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}
