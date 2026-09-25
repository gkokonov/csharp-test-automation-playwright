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

    public int PostLoginDelayMs { get; set; } = 5_000;

    public List<BootstrapUserDTO> Users { get; set; } = [];
}

public class BootstrapUserDTO
{
    public string Key { get; set; } = "default";

    public string? Username { get; set; }

    public string? Password { get; set; }
}
