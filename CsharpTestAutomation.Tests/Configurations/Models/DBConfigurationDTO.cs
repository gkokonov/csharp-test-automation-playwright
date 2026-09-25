namespace CsharpTestAutomation.Tests.Configurations.Models;

public class DBConfigurationDTO
{
    public string? Server { get; set; }
    public int Port { get; set; } = 5432; // Default PostgreSQL port
    public List<DatabaseInfo> Database { get; set; } = [];
    public PostgreSqlSettings PostgreSql { get; set; } = new();
}

public class DatabaseInfo
{
    public string? DbName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class PostgreSqlSettings
{
    public PasswordRefreshSettings PasswordRefresh { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public bool EnableSslMode { get; set; } = true;
    public string SslMode { get; set; } = "Require";
    public bool TrustServerCertificate { get; set; } = true;
}

public class PasswordRefreshSettings
{
    public int SuccessRefreshIntervalMinutes { get; set; } = 10;
    public int FailureRefreshIntervalSeconds { get; set; } = 5;
}

public class RetrySettings
{
    public int MaxRetryCount { get; set; } = 5;
    public int MaxRetryDelaySeconds { get; set; } = 10;
}
