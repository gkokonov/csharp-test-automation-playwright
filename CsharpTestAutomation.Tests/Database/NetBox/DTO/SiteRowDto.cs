namespace CsharpTestAutomation.Tests.Database.NetBox.DTO;

public sealed record SiteRowDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
