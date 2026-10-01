using Bogus;

namespace CsharpTestAutomation.Tests.TestData.NetBox;

/// <summary>
/// Shared, parallel-safe defaults for NetBox test data, generated through Bogus rather than raw
/// <see cref="Guid"/>/<see cref="Random"/> calls. Bogus's <see cref="Randomizer"/> primitives
/// (the <c>Faker.Random</c> facet used below) are documented as thread-safe, so one shared
/// <see cref="Faker"/> instance is safe across concurrently executing test fixtures. Every
/// name/slug/prefix/IP is generated per call so concurrent tests never collide on a fixed
/// identifier.
/// </summary>
public static class NetBoxTestData
{
    private static readonly Faker s_faker = new();

    public const string DefaultDescription = "automation test data";
    public const string UpdatedDescription = "automation test data (updated)";
    public const string ActiveStatus = "active";
    public const string PlannedStatus = "planned";

    /// <summary>
    /// Builds a NetBox-slug-legal identifier (lowercase, hyphen-separated) suitable for both the
    /// <c>name</c> and <c>slug</c> fields, so the two are never generated independently.
    /// </summary>
    public static string NewName(string feature) => $"auto-{feature}-{s_faker.Random.Hash(32)}";

    /// <summary>
    /// Generates a unique RFC1918 /24 prefix in the 10.0.0.0/8 range.
    /// </summary>
    public static string NewPrefix() => $"10.{s_faker.Random.Byte()}.{s_faker.Random.Byte()}.0/24";

    /// <summary>
    /// Generates a unique RFC1918 host address in the 10.0.0.0/8 range.
    /// </summary>
    public static string NewIpAddress() =>
        $"10.{s_faker.Random.Byte()}.{s_faker.Random.Byte()}.{s_faker.Random.Number(1, 254)}/32";
}
