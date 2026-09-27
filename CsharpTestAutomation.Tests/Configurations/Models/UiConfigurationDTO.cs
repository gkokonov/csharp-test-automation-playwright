namespace CsharpTestAutomation.Tests.Configurations.Models;

public class UiConfigurationDTO
{
    public string? ApplicationUrl { get; set; }

    public UiBootstrapAuthConfigurationDTO Authentication { get; set; } = new();
}

public class UiBootstrapAuthConfigurationDTO
{
    public string StorageStateDirectory { get; set; } = "playwright/.auth";

    public int LoginTimeoutInMs { get; set; } = 30_000;

    /// <summary>
    /// Retained so older configuration files still bind. Bootstrap no longer waits this many
    /// milliseconds. It waits until the MSAL access token appears, up to
    /// <see cref="LoginTimeoutInMs"/>.
    /// </summary>
    [Obsolete("Bootstrap waits for the MSAL access token. This delay is not used.")]
    public int PostLoginDelayMs { get; set; } = 5_000;

    public List<BootstrapUserDTO> Users { get; set; } = [];
}

public class BootstrapUserDTO
{
    public string Key { get; set; } = "default";

    public string? Username { get; set; }

    public string? Password { get; set; }
}
