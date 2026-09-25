using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.CPF.DTO;
using Dapper;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.CPF.Queries;

public static class CpfQueries
{
    private const string SelectAllCpfsSql =
        @"SELECT c.id,
                 c.p_code                  AS pcode,
                 c.title,
                 c.description,
                 c.country_code            AS countrycode,
                 co.name                   AS countryname,
                 pi.coverage_start_year    AS coveragestartyear,
                 pi.coverage_end_year      AS coverageendyear,
                 c.stage,
                 (SELECT tm.user_id
                    FROM team_member tm
                   WHERE tm.cpf_id = c.id
                     AND tm.is_adm_responsible = true
                     AND tm.flagged_for_deletion = false
                   LIMIT 1)                AS teamleadid
          FROM cpfs c
          JOIN countries co ON co.code = c.country_code
          LEFT JOIN cpf_product_information pi ON pi.cpf_id = c.id";

    private const string SelectCpfByIdSql = SelectAllCpfsSql + " WHERE c.id = @Id";

    public static List<CpfRow> SelectAllCpfs()
    {
        using NpgsqlConnection connection = CpfDbBase.GetConnection();
        return DapperActions.QueryAll<CpfRow>(connection, SelectAllCpfsSql);
    }

    public static CpfRow SelectCpfById(Guid id)
    {
        using NpgsqlConnection connection = CpfDbBase.GetConnection();
        return DapperActions.Query<CpfRow>(connection, SelectCpfByIdSql, new DynamicParameters(new { Id = id }));
    }
}
