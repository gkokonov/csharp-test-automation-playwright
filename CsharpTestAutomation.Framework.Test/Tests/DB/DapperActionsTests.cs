using Allure.NUnit;
using CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;
using Dapper;
using Moq;

namespace CsharpTestAutomation.Framework.Test.Tests.DB;

[TestFixture]
[AllureNUnit]
[Category("DapperActionsTests")]
public class DapperActionsTests
{
    private static InMemoryDatabase s_db;
    private static Mock<IDatabaseConnectionFactory> s_connectionFactoryMock;
    private static UserRepository s_userRepository;

    private static readonly List<User> s_usersMockData =
    [
        new User {Id = 1, FirstName = "Ivan", SirName = "Ivanov", Title = "Mr.", Country = "Bulgaria", City = "Sofia", Email = "ivan_ivanov@abv.bg"},
        new User {Id = 2, FirstName = "Silvia", SirName = "Harrington", Title = "Mrs.", Country = "Bulgaria", City = "Ruse", Email = "silviaH@gmail.com"},
        new User {Id = 3, FirstName = "Peter", SirName = "Wright", Title = "Mr.", Country = "USA", City = "Los Angeles", Email = "p_wright@gmail.com"},
        new User {Id = 4, FirstName = "Margaret", SirName = "Johnson", Title = "Mrs.", Country = "UK", City = "London", Email = "margaret_j@gmail.com"},
        new User {Id = 5, FirstName = "Gianluca", SirName = "Ena", Title = "Mr.", Country = "Italy", City = "Rome", Email = "g_luca_ena@gmail.com"},
        new User {Id = 6, FirstName = "John", SirName = "Doe", Title = "Mr.", Country = "USA", City = "Decatur", Email = "john_doe@gmail.com"}
    ];

    [OneTimeSetUp]
    public static void SetupDatabase()
    {
        s_db = new InMemoryDatabase();
        s_connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
        s_userRepository = new UserRepository(s_connectionFactoryMock.Object);

        s_db.Insert(s_usersMockData); // Change this line to use an instance reference
        s_connectionFactoryMock.Setup(c => c.GetConnection()).Returns(s_db.OpenConnection());
    }

    [Test]
    public void ShouldBeAbleToReadFromDatabaseQueryingSingleEntityWithGenericReturnType()
    {
        var expectedUser = new User {
            Id = 1,
            FirstName = "Ivan",
            SirName = "Ivanov",
            Title = "Mr.",
            Country = "Bulgaria",
            City = "Sofia",
            Email = "ivan_ivanov@abv.bg"
        };

        const string readSqlQuery = @"
        SELECT
            Id,
            FirstName,
            SirName,
            Title,
            Country,
            City,
            Email
        FROM User
        WHERE FirstName = @firstName";

        var dpRead = new DynamicParameters();
        dpRead.Add("firstName", "Ivan");

        User? actualUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(actualUser, Is.EqualTo(expectedUser));
    }

    [Test]
    public void ShouldBeAbleToReadFromDatabaseQueryingSingleEntityWithDictionaryReturnType()
    {
        var expectedDictionary = new Dictionary<string, object>()
        {
            { "Id", 1 },
            { "FirstName", "Ivan" },
            { "SirName", "Ivanov" },
            { "Title", "Mr." },
            { "Country", "Bulgaria" },
            { "City", "Sofia" },
            { "Email", "ivan_ivanov@abv.bg" }
        };

        const string readSqlQuery = @"
        SELECT
            Id,
            FirstName,
            SirName,
            Title,
            Country,
            City,
            Email
        FROM User
        WHERE FirstName = @firstName";

        var dpRead = new DynamicParameters();
        dpRead.Add("firstName", "Ivan");

        IDictionary<string, object>? actualDictionary = s_userRepository.Query(readSqlQuery, dpRead);

        Assert.That(actualDictionary, Is.EqualTo(expectedDictionary));
    }

    [Test]
    public void ShouldBeAbleToReadFromDatabaseQueryingMultipleEntitiesWithGenericReturnType()
    {
        const int expectedUsersCount = 6;

        const string readSqlQuery = @"
        SELECT
            *
        FROM User";

        List<User> actualUsers = s_userRepository.QueryAll<User>(readSqlQuery);

        Assert.That(actualUsers, Has.Count.EqualTo(expectedUsersCount));
    }

    [Test]
    public void ShouldBeAbleToReadFromDatabaseQueryingMultipleEntitiesWithDictionaryReturnType()
    {
        const int expectedUsersCount = 6;

        const string readSqlQuery = @"
        SELECT
            *
        FROM User";

        List<IDictionary<string, object>> actualDictionaryList = s_userRepository.QueryAll(readSqlQuery);

        Assert.That(actualDictionaryList, Has.Count.EqualTo(expectedUsersCount));
    }

    [Test]
    public void ShouldBeAbleToCreateEntityInDatabase()
    {
        var expectedUser = new User {
            Id = 7,
            FirstName = "Frank",
            SirName = "Sinatra",
            Title = "Mr.",
            Country = "USA",
            City = "Hoboken",
            Email = "f_sinatra@gmail.com"
        };

        const string createSqlQuery = @"
        INSERT INTO
            User ( Id, FirstName, SirName, Title, Country, City, Email )
        VALUES
            ( @id, @firstName, @sirName, @title, @country, @city, @email )";

        const string readSqlQuery = @"
        SELECT
            Id,
            FirstName,
            SirName,
            Title,
            Country,
            City,
            Email
        FROM User
        WHERE FirstName = @firstName";

        const string deleteSqlQuery = @"
        DELETE
        FROM User
        WHERE Email = @email";

        var dpCreate = new DynamicParameters(
            new {
                id = 7,
                firstName = "Frank",
                sirName = "Sinatra",
                title = "Mr.",
                country = "USA",
                city = "Hoboken",
                email = "f_sinatra@gmail.com"
            }
        );

        var dpRead = new DynamicParameters();
        dpRead.Add("firstName", "Frank");

        var dpDelete = new DynamicParameters();
        dpDelete.Add("email", "f_sinatra@gmail.com");

        s_userRepository.Execute(createSqlQuery, dpCreate);

        User? actualUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(actualUser, Is.EqualTo(expectedUser));

        s_userRepository.Execute(deleteSqlQuery, dpDelete);

        User? deletedUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(deletedUser, Is.Null);
    }

    [Test]
    public void ShouldBeAbleToUpdateEntityInDatabase()
    {
        var expectedUser = new User {
            Id = 7,
            FirstName = "Frank",
            SirName = "Sinatra",
            Title = "Mr.",
            Country = "USA",
            City = "Hoboken",
            Email = "f_sinatra@gmail.com"
        };

        const string createSqlQuery = @"
        INSERT INTO
            User ( Id, FirstName, SirName, Title, Country, City, Email )
        VALUES
            ( @id, @firstName, @sirName, @title, @country, @city, @email )";

        const string readSqlQuery = @"
        SELECT
            Id,
            FirstName,
            SirName,
            Title,
            Country,
            City,
            Email
        FROM User
        WHERE FirstName = @firstName";

        const string updateSqlQuery = @"
        UPDATE
            User
        SET
            Id = @id,
            FirstName = @firstName,
            SirName = @sirName,
            Title = @title,
            Country = @country,
            City = @city,
            Email = @email
        WHERE SirName = @sirName";

        const string deleteSqlQuery = @"
        DELETE
        FROM User
        WHERE Email = @email";

        var dpCreate = new DynamicParameters(
            new {
                id = 7,
                firstName = "Frank",
                sirName = "Sinatra",
                title = "Mr.",
                country = "USA",
                city = "Hoboken",
                email = "f_sinatra@gmail.com"
            }
        );

        var dpRead = new DynamicParameters();
        dpRead.Add("firstName", "Frank");

        var dpUpdate = new DynamicParameters(
            new {
                id = 7,
                firstName = "Frank",
                sirName = "Sinatra",
                title = "Mr.",
                country = "Bulgaria",
                city = "Sofia",
                email = "f_sinatra@gmail.com"
            }
        );

        var dpDelete = new DynamicParameters();
        dpDelete.Add("email", "f_sinatra@gmail.com");

        s_userRepository.Execute(createSqlQuery, dpCreate);

        User? actualUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(actualUser, Is.EqualTo(expectedUser));

        s_userRepository.Execute(updateSqlQuery, dpUpdate);

        expectedUser.Country = "Bulgaria";
        expectedUser.City = "Sofia";

        User? updatedUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(updatedUser, Is.EqualTo(expectedUser));

        s_userRepository.Execute(deleteSqlQuery, dpDelete);

        User? deletedUser = s_userRepository.Query<User>(readSqlQuery, dpRead);

        Assert.That(deletedUser, Is.Null);
    }
}
