using Bogus;

namespace CsharpTestAutomation.Tests.TestData.NetBox;

/// <summary>
/// Shared, parallel-safe defaults for NetBox test data, generated through Bogus rather than raw
/// <see cref="Guid"/>/<see cref="Random"/> calls. Bogus's <see cref="Randomizer"/> primitives
/// (the <c>Faker.Random</c> facet used below) are documented as thread-safe, so one shared
/// <see cref="Faker"/> instance is safe across concurrently executing test fixtures. Every
/// name/slug/prefix/IP is generated per call. Prefix allocations are retained for the process
/// lifetime so concurrent fixtures cannot reuse a CIDR within one test run.
/// </summary>
public static class NetBoxTestData
{
    private static readonly Faker s_faker = new();
    private static readonly HashSet<string> s_prefixes = [];
    private static readonly object s_prefixLock = new();

    public const string DefaultDescription = "automation test data";
    public const string UpdatedDescription = "automation test data (updated)";
    public const string ActiveStatus = "active";
    public const string PlannedStatus = "planned";
    public const string ReservedStatus = "reserved";

    /// <summary>
    /// Builds a NetBox-slug-legal identifier (lowercase, hyphen-separated) suitable for both the
    /// <c>name</c> and <c>slug</c> fields, so the two are never generated independently.
    /// </summary>
    public static string NewName(string feature) => $"auto-{feature}-{s_faker.Random.Hash(32)}";

    /// <summary>
    /// Generates an RFC1918 /24 prefix not previously issued in this process.
    /// </summary>
    public static string NewPrefix()
    {
        lock (s_prefixLock)
        {
            if (s_prefixes.Count == 65536)
            {
                throw new InvalidOperationException("All automation /24 prefixes in 10.0.0.0/8 have been allocated.");
            }

            string prefix;
            do
            {
                prefix = $"10.{s_faker.Random.Byte()}.{s_faker.Random.Byte()}.0/24";
            }
            while (!s_prefixes.Add(prefix));

            return prefix;
        }
    }

    /// <summary>Generates a host inside a /24 prefix issued by <see cref="NewPrefix"/>.</summary>
    public static string NewIpAddress(string prefix)
    {
        System.Net.IPNetwork network = System.Net.IPNetwork.Parse(prefix);
        if (network.PrefixLength != 24 || network.BaseAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new ArgumentException("An IPv4 /24 prefix is required.", nameof(prefix));
        }

        byte[] octets = network.BaseAddress.GetAddressBytes();
        octets[3] = (byte)s_faker.Internet.Random.Number(1, 254);
        return $"{new System.Net.IPAddress(octets)}/24";
    }

    /// <summary>
    /// Generates a random RFC1918 host address in the 10.0.0.0/8 range.
    /// </summary>
    public static string NewIpAddress() =>
        $"10.{s_faker.Random.Byte()}.{s_faker.Random.Byte()}.{s_faker.Random.Number(1, 254)}/32";
}
