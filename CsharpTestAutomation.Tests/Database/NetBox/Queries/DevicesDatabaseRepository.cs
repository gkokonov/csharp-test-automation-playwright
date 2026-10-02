using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class DevicesDatabaseRepository
{
    public static DeviceRowDto? GetByName(string name)
    {
        const string sql = """
            SELECT id, name, site_id AS "SiteId", status
            FROM dcim_device
            WHERE name = @Name
            """;

        return NetBoxDatabase.Run(connection =>
            DapperActions.Query<DeviceRowDto>(connection, sql, new DynamicParameters(new { Name = name })));
    }
}
