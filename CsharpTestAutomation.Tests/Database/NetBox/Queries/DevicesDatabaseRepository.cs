using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class DevicesDatabaseRepository
{
    public static DeviceRowDto? GetByName(string name)
    {
        const string sql = """
            SELECT id AS "Id", name AS "Name", site_id AS "SiteId",
                device_type_id AS "DeviceTypeId", role_id AS "RoleId",
                status AS "Status", description AS "Description"
            FROM dcim_device
            WHERE name = @Name
            """;

        return NetBoxDatabase.Run(connection =>
            DapperActions.Query<DeviceRowDto>(connection, sql, new DynamicParameters(new { Name = name })));
    }
}
