using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;

public class InMemoryDatabase : IDisposable
{
    private readonly OrmLiteConnection _connection = (OrmLiteConnection)
        new OrmLiteConnectionFactory(":memory:", SqliteOrmLiteDialectProvider.Instance).OpenDbConnection();

    public IDbConnection OpenConnection() => _connection;

    // OrmLite deliberately does not dispose shared :memory: connections through its wrapper.
    public void Dispose() => _connection.DbConnection.Dispose();

    public void Insert<T>(IEnumerable<T> items)
    {
        using IDbConnection db = OpenConnection();
        db.CreateTableIfNotExists<T>();
        foreach (T? item in items)
        {
            db.Insert(item);
        }
    }
}
