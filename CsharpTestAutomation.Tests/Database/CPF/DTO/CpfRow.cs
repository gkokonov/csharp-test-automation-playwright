namespace CsharpTestAutomation.Tests.Database.CPF.DTO;

public record CpfRow(
    Guid Id,
    string? PCode,
    string Title,
    string? Description,
    string CountryCode,
    string? CountryName,
    int? CoverageStartYear,
    int? CoverageEndYear,
    string Stage,
    string? TeamLeadId);
