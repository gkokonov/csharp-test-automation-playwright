using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Configurations.Models;

namespace CsharpTestAutomation.Tests.Configurations;

public class ExtendedConfiguration : CoreConfiguration
{
    #region Database

    public DBConfigurationDTO? DbSettings { get; set; }

    #endregion Database

    #region Entra ID

    public EntraIdConfigurationDTO? EntraIdSettings { get; set; }

    #endregion Entra ID

    #region UI

    public UiConfigurationDTO? Ui { get; set; }

    #endregion UI
}
