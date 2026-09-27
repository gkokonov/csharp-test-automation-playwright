namespace CsharpTestAutomation.Tests.Database.NetBox.DTO;

public sealed record PrefixRowDto
{
    public long Id { get; init; }
    public string Prefix { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
