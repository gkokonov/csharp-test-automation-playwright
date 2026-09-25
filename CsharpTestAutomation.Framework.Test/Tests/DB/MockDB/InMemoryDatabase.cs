using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;

public class InMemoryDatabase
{
    private readonly OrmLiteConnectionFactory _dbFactory = new(":memory:", SqliteOrmLiteDialectProvider.Instance);

    public IDbConnection OpenConnection() => _dbFactory.OpenDbConnection();

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
