using System.Data;

namespace CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;

public interface IDatabaseConnectionFactory
{
    IDbConnection GetConnection();
}