using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Tests.Configurations.Models;
using CsharpTestAutomation.Tests.Database;
using Npgsql;

namespace CsharpTestAutomation.Tests.UnitTests;

[TestFixture]
[AllureNUnit]
[AllureSuite("Local infrastructure")]
[AllureFeature("PostgreSQL configuration")]
public class PostgreSqlConnectionPoolTests
{
    [TestCase("part;two")]
    [TestCase("quoted\"password")]
    [TestCase(" spaced password ")]
    [AllureStory("Connection values round-trip without changing pool options")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public void Verify_ConfigurationRoundTrips_When_ValuesNeedEscaping(string password)
    {
        // Arrange
        var database = new DatabaseInfo { DbName = "test;database", Username = "test;user", Password = password };
        var settings = new DBConfigurationDTO { Server = "localhost", Port = 5433 };

        // Act
        string text = PostgreSqlConnectionPool.BuildConnectionString(database, settings);
        var result = new NpgsqlConnectionStringBuilder(text);

        // Assert
        using (new AssertionScope())
        {
            result.Host.Should().Be("localhost");
            result.Port.Should().Be(5433);
            result.Database.Should().Be(database.DbName);
            result.Username.Should().Be(database.Username);
            result.Password.Should().Be(password);
            result.SslMode.Should().Be(SslMode.Require);
            result["Trust Server Certificate"].Should().Be(true);
            result.Pooling.Should().BeTrue();
            result.MinPoolSize.Should().Be(1);
            result.MaxPoolSize.Should().Be(20);
            result.CommandTimeout.Should().Be(30);
        }
    }

    [Test]
    [AllureStory("Provider defaults apply when SSL mode is not configured")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public void Verify_DefaultSslOptionsPreserved_When_ExplicitSslModeIsDisabled()
    {
        // Arrange
        var database = new DatabaseInfo { DbName = "test", Username = "user", Password = "password" };
        var settings = new DBConfigurationDTO { Server = "localhost" };
        settings.PostgreSql.EnableSslMode = false;

        // Act
        string text = PostgreSqlConnectionPool.BuildConnectionString(database, settings);
        var result = new NpgsqlConnectionStringBuilder(text);

        // Assert
        result.SslMode.Should().Be(new NpgsqlConnectionStringBuilder().SslMode);
    }
}
