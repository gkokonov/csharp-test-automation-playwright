namespace CsharpTestAutomation.Tests.Database.NetBox.DTO;

public sealed record DeviceRowDto
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public long SiteId { get; init; }
    public string Status { get; init; } = string.Empty;
}
