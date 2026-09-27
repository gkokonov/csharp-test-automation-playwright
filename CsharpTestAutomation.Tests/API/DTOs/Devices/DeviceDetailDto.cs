using System.Text.Json.Serialization;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;

namespace CsharpTestAutomation.Tests.Api.Dtos.Devices;

/// <summary>
/// GET /api/dcim/devices/{id}/ response. Also used as the create/update response and as the item
/// shape of GET /api/dcim/devices/ (NetBox uses the same schema for detail and list item).
/// </summary>
public sealed record DeviceDetailDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("site")]
    public BriefSiteDto Site { get; set; } = new();

    [JsonPropertyName("device_type")]
    public BriefDeviceTypeDto DeviceType { get; set; } = new();

    [JsonPropertyName("role")]
    public BriefDeviceRoleDto Role { get; set; } = new();

    [JsonPropertyName("status")]
    public StatusFieldDto Status { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
