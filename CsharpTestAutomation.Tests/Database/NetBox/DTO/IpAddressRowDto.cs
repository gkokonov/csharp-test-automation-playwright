namespace CsharpTestAutomation.Tests.Database.NetBox.DTO;

public sealed record IpAddressRowDto
{
    public long Id { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
