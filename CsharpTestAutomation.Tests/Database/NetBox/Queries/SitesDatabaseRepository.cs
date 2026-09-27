using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class SitesDatabaseRepository
{
    public static SiteRowDto? GetBySlug(NpgsqlConnection connection, string slug)
    {
        const string sql = """
            SELECT id, name, slug, status
            FROM dcim_site
            WHERE slug = @Slug
            """;

        return DapperActions.Query<SiteRowDto>(connection, sql, new DynamicParameters(new { Slug = slug }));
    }
}
