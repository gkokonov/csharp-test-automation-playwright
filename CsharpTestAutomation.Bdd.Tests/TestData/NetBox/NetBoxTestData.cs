using Bogus;

namespace CsharpTestAutomation.Bdd.Tests.TestData.NetBox;

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

    public static string NewName(string feature) => $"auto-bdd-{feature}-{s_faker.Random.Hash(32)}";

    /// <summary>Reserves a generated RFC1918 /24 for this process lifetime.</summary>
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

    public static string NewIpAddress() =>
        $"10.{s_faker.Random.Byte()}.{s_faker.Random.Byte()}.{s_faker.Random.Number(1, 254)}/32";
}
