using System.Data;
using CsharpTestAutomation.Framework.Common;
using Dapper;

namespace CsharpTestAutomation.Framework.DB;

/// <summary>
/// Dapper based DB capabilities.
/// </summary>
public static class DapperActions
{
    private static readonly CoreConfiguration s_config = AppConfiguration<CoreConfiguration>.Instance.Settings;

    /// <summary>
    /// Execute CREATE, UPDATE and DELETE SQL queries.
    /// </summary>
    /// <param name="dbConnection">The connection to query on.</param>
    /// <param name="sqlQuery">The SQL to execute for this query.</param>
    /// <param name="parameters">The parameters to pass, if any.</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>The number of rows affected.</returns>
    public static int Execute(IDbConnection dbConnection, string sqlQuery, DynamicParameters? parameters = null,
        CommandType commandType = CommandType.Text)
    {
        return dbConnection.Execute(sqlQuery, parameters ?? new DynamicParameters(), commandTimeout: s_config.DbExecuteTimeoutSeconds,
            commandType: commandType);
    }

    /// <summary>
    /// Executes a READ SQL query, returning the data typed as a <typeparamref name="T"/> entity.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="dbConnection">The connection to query on.</param>
    /// <param name="sqlQuery">The SQL to execute for this query.</param>
    /// <param name="parameters">The parameters to pass, if any.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds. Defaults to <see cref="CoreConfiguration.DbQueryTimeoutSeconds"/> when null.</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>Single entity based on the parameter T that is passed.</returns>
    public static T? Query<T>(IDbConnection dbConnection, string sqlQuery, DynamicParameters? parameters = null,
        int? timeoutInSeconds = null, CommandType commandType = CommandType.Text)
    {
        return dbConnection.QueryFirstOrDefault<T>(sqlQuery, parameters ?? new DynamicParameters(),
            commandTimeout: timeoutInSeconds ?? s_config.DbQueryTimeoutSeconds, commandType: commandType);
    }

    /// <summary>
    /// Executes a READ SQL query, returning the data typed as an IDictionary object with column
    /// name as Key(string) and Value(object).
    /// </summary>
    /// <param name="dbConnection">The connection to query on.</param>
    /// <param name="sqlQuery">The SQL to execute for this query.</param>
    /// <param name="parameters">The parameters to pass, if any.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds. Defaults to <see cref="CoreConfiguration.DbQueryTimeoutSeconds"/> when null.</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static IDictionary<string, object>? Query(IDbConnection dbConnection, string sqlQuery,
        DynamicParameters? parameters = null, int? timeoutInSeconds = null, CommandType commandType = CommandType.Text)
    {
        return dbConnection.QueryFirstOrDefault<object>(sqlQuery, parameters ?? new DynamicParameters(),
            commandTimeout: timeoutInSeconds ?? s_config.DbQueryTimeoutSeconds, commandType: commandType)
            as IDictionary<string, object>;
    }

    /// <summary>
    /// Executes a READ SQL query, returning the data typed as a List of <typeparamref name="T"/> entities.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="dbConnection">The connection to query on.</param>
    /// <param name="sqlQuery">The SQL to execute for this query.</param>
    /// <param name="parameters">The parameters to pass, if any.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds. Defaults to <see cref="CoreConfiguration.DbQueryTimeoutSeconds"/> when null.</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>Multiple entities as a list based on the parameter T that is passed.</returns>
    public static List<T> QueryAll<T>(IDbConnection dbConnection, string sqlQuery, DynamicParameters? parameters = null,
        int? timeoutInSeconds = null, CommandType commandType = CommandType.Text)
    {
        return [.. dbConnection.Query<T>(sqlQuery, parameters ?? new DynamicParameters(),
            commandTimeout: timeoutInSeconds ?? s_config.DbQueryTimeoutSeconds,
            commandType: commandType)];
    }

    /// <summary>
    /// Executes a READ SQL query, returning the data typed as a List of IDictionary object with
    /// column name as Key(string) and Value(object).
    /// </summary>
    /// <param name="dbConnection">The connection to query on.</param>
    /// <param name="sqlQuery">The SQL to execute for this query.</param>
    /// <param name="parameters">The parameters to pass, if any.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds. Defaults to <see cref="CoreConfiguration.DbQueryTimeoutSeconds"/> when null.</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static List<IDictionary<string, object>> QueryAll(IDbConnection dbConnection, string sqlQuery,
        DynamicParameters? parameters = null, int? timeoutInSeconds = null, CommandType commandType = CommandType.Text)
    {
        return [.. dbConnection.Query(sqlQuery, parameters ?? new DynamicParameters(),
            commandTimeout: timeoutInSeconds ?? s_config.DbQueryTimeoutSeconds,
            commandType: commandType).Cast<IDictionary<string, object>>()];
    }
}
