using System.Collections.Frozen;
using Allure.Net.Commons;

namespace CsharpTestAutomation.Bdd.Tests.TestData.NetBox;

/// <summary>Original NUnit story, severity, and owner contracts keyed by stable parity ID.</summary>
public static class IpamScenarioMetadata
{
    public const string Owner = "Automation Team";

    private static readonly FrozenDictionary<string, (string Story, SeverityLevel Severity)> s_scenarios =
        new Dictionary<string, (string, SeverityLevel)>
        {
            ["PrefixApi01"] = ("Creating a Prefix returns and persists the requested fields", SeverityLevel.critical),
            ["PrefixApi02"] = ("An exact CIDR filter returns only the matching Prefix", SeverityLevel.normal),
            ["PrefixApi03"] = ("An active Prefix can be reserved without changing its CIDR", SeverityLevel.normal),
            ["PrefixApi04"] = ("Deleting a Prefix removes it from REST and PostgreSQL", SeverityLevel.critical),
            ["IpAddressApi01"] = ("Creating a IP Address returns and persists the requested fields", SeverityLevel.critical),
            ["IpAddressApi02"] = ("An exact CIDR filter returns only the matching IP Address", SeverityLevel.normal),
            ["IpAddressApi03"] = ("An active IP Address can be reserved without changing its CIDR", SeverityLevel.normal),
            ["IpAddressApi04"] = ("Deleting a IP Address removes it from REST and PostgreSQL", SeverityLevel.critical),
            ["PrefixUi01"] = ("Creating a Prefix through the UI persists it across REST API and PostgreSQL", SeverityLevel.critical)
        }.ToFrozenDictionary();

    public static (string Story, SeverityLevel Severity) Get(IEnumerable<string> tags) =>
        s_scenarios[tags.Single(s_scenarios.ContainsKey)];
}
