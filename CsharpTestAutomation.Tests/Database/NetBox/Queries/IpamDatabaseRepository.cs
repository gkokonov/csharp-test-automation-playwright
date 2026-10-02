using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class IpamDatabaseRepository
{
    public static PrefixRowDto? GetPrefixByCidr(string prefix)
    {
        const string sql = """
            SELECT id, prefix::text AS "Prefix", status, description
            FROM ipam_prefix
            WHERE prefix = @Prefix::cidr
            """;

        return NetBoxDatabase.Run(connection =>
            DapperActions.Query<PrefixRowDto>(connection, sql, new DynamicParameters(new { Prefix = prefix })));
    }

    public static IpAddressRowDto? GetIpAddressByAddress(string address)
    {
        const string sql = """
            SELECT id, host(address) AS "Address", status, description
            FROM ipam_ipaddress
            WHERE address = @Address::inet
            """;

        return NetBoxDatabase.Run(connection =>
            DapperActions.Query<IpAddressRowDto>(connection, sql, new DynamicParameters(new { Address = address })));
    }
}
