using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class IpamDatabaseRepository
{
    public static PrefixRowDto? GetPrefixByCidr(NpgsqlConnection connection, string prefix)
    {
        const string sql = """
            SELECT id, prefix::text AS "Prefix", status, description
            FROM ipam_prefix
            WHERE prefix = @Prefix::cidr
            """;

        return DapperActions.Query<PrefixRowDto>(connection, sql, new DynamicParameters(new { Prefix = prefix }));
    }

    public static IpAddressRowDto? GetIpAddressByAddress(NpgsqlConnection connection, string address)
    {
        const string sql = """
            SELECT id, host(address) AS "Address", status, description
            FROM ipam_ipaddress
            WHERE address = @Address::inet
            """;

        return DapperActions.Query<IpAddressRowDto>(connection, sql, new DynamicParameters(new { Address = address }));
    }
}
