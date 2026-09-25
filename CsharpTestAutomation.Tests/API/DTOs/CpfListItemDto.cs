using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos;

public record CpfListItemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("pCode")] string PCode,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("countryCode")] string CountryCode,
    [property: JsonPropertyName("countryName")] string? CountryName,
    [property: JsonPropertyName("coverageStartYear")] int? CoverageStartYear,
    [property: JsonPropertyName("coverageEndYear")] int? CoverageEndYear,
    [property: JsonPropertyName("stage")] string Stage,
    [property: JsonPropertyName("teamLeadId")] string? TeamLeadId,
    [property: JsonPropertyName("teamLeadName")] string? TeamLeadName);
