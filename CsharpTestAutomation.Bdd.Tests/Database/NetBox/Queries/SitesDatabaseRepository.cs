using CsharpTestAutomation.Bdd.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;

public sealed class SitesDatabaseRepository(PostgreSqlConnectionPool connections, int queryTimeoutSeconds)
{
    private readonly NetBoxDatabase _database = new(connections);

    public Task<SiteRowDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id", name AS "Name", slug AS "Slug",
                status AS "Status", description AS "Description"
            FROM dcim_site
            WHERE slug = @Slug
            """;

        return _database.RunAsync(connection => connection.QuerySingleOrDefaultAsync<SiteRowDto>(
            new CommandDefinition(sql, new { Slug = slug }, commandTimeout: queryTimeoutSeconds,
                cancellationToken: cancellationToken)), cancellationToken);
    }
}
