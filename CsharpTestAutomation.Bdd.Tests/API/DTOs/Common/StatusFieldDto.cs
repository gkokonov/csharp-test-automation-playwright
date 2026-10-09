using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Bdd.Tests.Api.Dtos.Common;

/// <summary>
/// NetBox's nested value/label representation of a choice field (e.g. status) in read responses.
/// </summary>
public sealed record StatusFieldDto
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
