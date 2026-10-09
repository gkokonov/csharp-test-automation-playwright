using System.Collections.Frozen;
using Allure.Net.Commons;

namespace CsharpTestAutomation.Bdd.Tests.TestData.NetBox;

/// <summary>Original NUnit story, severity, and owner contracts keyed by stable parity ID.</summary>
public static class SiteScenarioMetadata
{
    public const string Owner = "Automation Team";

    private static readonly FrozenDictionary<string, (string Story, SeverityLevel Severity)> s_scenarios =
        new Dictionary<string, (string, SeverityLevel)>
        {
            ["SiteApi01"] = ("Creating a site returns the created representation", SeverityLevel.critical),
            ["SiteApi02"] = ("A created site is persisted and retrievable through both the REST API and PostgreSQL", SeverityLevel.critical),
            ["SiteApi03"] = ("A created site can be found by filtering on its slug", SeverityLevel.normal),
            ["SiteApi04"] = ("Updating a site's status and description leaves its other fields unchanged", SeverityLevel.normal),
            ["SiteApi05"] = ("Deleting a site removes it so a subsequent lookup returns 404", SeverityLevel.critical),
            ["SiteUi01"] = ("Creating a site through the UI persists across UI, REST API, and PostgreSQL layers", SeverityLevel.critical),
            ["SiteUi02"] = ("Updating a site through the UI reflects the changed status across UI, REST API, and PostgreSQL layers", SeverityLevel.normal),
            ["SiteUi03"] = ("Deleting a site through the UI removes it across UI, REST API, and PostgreSQL layers", SeverityLevel.critical)
        }.ToFrozenDictionary();

    public static (string Story, SeverityLevel Severity) Get(IEnumerable<string> tags) =>
        s_scenarios[tags.Single(s_scenarios.ContainsKey)];
}
