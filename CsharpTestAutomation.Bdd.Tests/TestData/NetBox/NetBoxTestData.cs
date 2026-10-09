using Bogus;

namespace CsharpTestAutomation.Bdd.Tests.TestData.NetBox;

public static class NetBoxTestData
{
    private static readonly Faker s_faker = new();

    public const string DefaultDescription = "automation test data";
    public const string UpdatedDescription = "automation test data (updated)";
    public const string ActiveStatus = "active";
    public const string PlannedStatus = "planned";

    public static string NewName(string feature) => $"auto-bdd-{feature}-{s_faker.Random.Hash(32)}";
}
