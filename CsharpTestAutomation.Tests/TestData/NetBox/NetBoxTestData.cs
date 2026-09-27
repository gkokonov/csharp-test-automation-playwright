namespace CsharpTestAutomation.Tests.TestData.NetBox;

/// <summary>
/// Shared, parallel-safe defaults for NetBox test data. Every name/slug is generated per call so
/// concurrent tests never collide on a fixed identifier.
/// </summary>
public static class NetBoxTestData
{
    public const string DefaultDescription = "automation test data";
    public const string ActiveStatus = "active";

    /// <summary>
    /// Builds a NetBox-slug-legal identifier (lowercase, hyphen-separated) suitable for both the
    /// <c>name</c> and <c>slug</c> fields, so the two are never generated independently.
    /// </summary>
    public static string NewName(string feature) => $"auto-{feature}-{Guid.NewGuid():N}";

    /// <summary>
    /// Generates a unique RFC1918 /24 prefix in the 10.0.0.0/8 range.
    /// </summary>
    public static string NewPrefix()
    {
        var random = Random.Shared;
        return $"10.{random.Next(0, 256)}.{random.Next(0, 256)}.0/24";
    }

    /// <summary>
    /// Generates a unique RFC1918 host address in the 10.0.0.0/8 range.
    /// </summary>
    public static string NewIpAddress()
    {
        var random = Random.Shared;
        return $"10.{random.Next(0, 256)}.{random.Next(0, 256)}.{random.Next(1, 255)}/32";
    }
}
