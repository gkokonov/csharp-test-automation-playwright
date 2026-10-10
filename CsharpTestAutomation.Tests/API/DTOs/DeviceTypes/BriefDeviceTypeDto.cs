using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;

/// <summary>
/// NetBox's brief (nested) representation of a device type, as returned inside a device.
/// </summary>
public sealed record BriefDeviceTypeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}

