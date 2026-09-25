using CsharpTestAutomation.Framework.DB;
using Dapper;

namespace CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;

public class UserRepository(IDatabaseConnectionFactory connectionFactory)
{
    private readonly IDatabaseConnectionFactory _connectionFactory = connectionFactory;

    public int Execute(string sqlQuery, DynamicParameters parameters = null)
    {
        return DapperActions.Execute(_connectionFactory.GetConnection(), sqlQuery, parameters);
    }

    public T Query<T>(string sqlQuery, DynamicParameters parameters = null)
    {
        return DapperActions.Query<T>(_connectionFactory.GetConnection(), sqlQuery, parameters);
    }

    public IDictionary<string, object> Query(string sqlQuery, DynamicParameters parameters = null)
    {
        return DapperActions.Query(_connectionFactory.GetConnection(), sqlQuery, parameters);
    }

    public List<T> QueryAll<T>(string sqlQuery, DynamicParameters parameters = null)
    {
        return DapperActions.QueryAll<T>(_connectionFactory.GetConnection(), sqlQuery, parameters);
    }

    public List<IDictionary<string, object>> QueryAll(string sqlQuery, DynamicParameters parameters = null)
    {
        return DapperActions.QueryAll(_connectionFactory.GetConnection(), sqlQuery, parameters);
    }
}