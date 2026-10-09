using System.Collections.Frozen;
using Allure.Net.Commons;

namespace CsharpTestAutomation.Bdd.Tests.TestData.NetBox;

/// <summary>Original NUnit story, severity, and owner contracts keyed by stable parity ID.</summary>
public static class DeviceScenarioMetadata
{
    public const string Owner = "Automation Team";

    private static readonly FrozenDictionary<string, (string Story, SeverityLevel Severity)> s_scenarios =
        new Dictionary<string, (string, SeverityLevel)>
        {
            ["DeviceApi01"] = ("Creating a Device returns its requested fields and persists them in PostgreSQL", SeverityLevel.critical),
            ["DeviceApi02"] = ("An exact Device name search returns the created Device", SeverityLevel.normal),
            ["DeviceApi03"] = ("PATCH changes Device status and description, and moves its site only when requested", SeverityLevel.normal),
            ["DeviceApi04"] = ("PATCH changes Device status and description, and moves its site only when requested", SeverityLevel.normal),
            ["DeviceApi05"] = ("Deleting a Device removes it from the REST API and PostgreSQL", SeverityLevel.critical),
            ["DeviceApi06"] = ("Filtering by site includes all owned matching Devices and excludes a Device at another site", SeverityLevel.normal),
            ["DeviceApi07"] = ("Cleanup removes owned records in dependency order after an interrupted scenario", SeverityLevel.critical),
            ["DeviceApi08"] = ("Cleanup removes owned records in dependency order after an interrupted scenario", SeverityLevel.critical),
            ["DeviceUi01"] = ("Creating a Device through the UI persists its fields across REST API and PostgreSQL", SeverityLevel.critical)
        }.ToFrozenDictionary();

    public static (string Story, SeverityLevel Severity) Get(IEnumerable<string> tags) =>
        s_scenarios[tags.Single(s_scenarios.ContainsKey)];
}
