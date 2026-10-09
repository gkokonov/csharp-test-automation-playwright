using System.Text.Json.Serialization;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Manufacturers;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceTypes;

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

/// <summary>
/// GET/POST /api/dcim/device-types/ response shape (list, detail, and create share one schema).
/// </summary>
public sealed record DeviceTypeDto
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

    [JsonPropertyName("manufacturer")]
    public BriefManufacturerDto Manufacturer { get; set; } = new();
}
