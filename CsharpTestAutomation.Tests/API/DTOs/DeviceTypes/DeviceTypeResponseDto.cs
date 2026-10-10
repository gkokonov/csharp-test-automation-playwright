using System.Text.Json.Serialization;
using CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;

namespace CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;

/// <summary>
/// GET/POST /api/dcim/device-types/ response shape (list, detail, and create share one schema).
/// </summary>
public sealed record DeviceTypeResponseDto
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

