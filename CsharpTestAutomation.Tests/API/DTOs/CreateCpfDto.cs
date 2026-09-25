using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos;

public record CreateCpfDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("coverageStartYear")]
    public int CoverageStartYear { get; set; }

    [JsonPropertyName("coverageEndYear")]
    public int? CoverageEndYear { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("teamLeadId")]
    public string? TeamLeadId { get; set; }
}
