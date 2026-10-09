using CsharpTestAutomation.Bdd.Tests.Configurations.Models;
using CsharpTestAutomation.Framework.Common;

namespace CsharpTestAutomation.Bdd.Tests.Configurations;

public sealed class ExtendedConfiguration : CoreConfiguration
{
    public DBConfigurationDTO? DbSettings { get; set; }

    public NetBoxConfigurationDTO NetBox { get; set; } = new();

    public UiConfigurationDTO Ui { get; set; } = new();
}
