namespace CsharpTestAutomation.Bdd.Tests.Configurations.Models;

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
    public bool EnableSslMode { get; set; } = true;
    public string SslMode { get; set; } = "Require";
}
