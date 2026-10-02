# DB Repository Connection Refactor

Guide to move DB query repositories from "the test supplies the connection" to "the repository owns the connection". Apply it to any application test repository that uses `DapperActions` and a `PostgreSqlConnectionPool`.

## Goal

- Tests call a query method and do not touch connections, the pool, or database names.
- Each query opens a short-lived connection and disposes it. No connection leaks.
- The database name exists in one place per application.
- Safe for parallel NUnit fixtures.

## Before and after

Before (test owns the connection):

```csharp
using NpgsqlConnection connection = PostgreSqlConnectionPool.Instance.GetConnection("netbox");
SiteRowDto? row = SitesDatabaseRepository.GetBySlug(connection, created.Slug);
```

After (repository owns the connection):

```csharp
SiteRowDto? row = SitesDatabaseRepository.GetBySlug(created.Slug);
```

## Design

| Decision | Reason |
| --- | --- |
| Add one internal `<App>Database` helper per application. | One owner for the database name and the connection lifetime. |
| The helper uses `using` and runs a `Func<NpgsqlConnection, T>`. | Callers cannot forget to dispose. |
| Use the database **name**, not an index such as `Database[2]`. | Index lookup breaks when the config array order changes. |
| Keep `DapperActions` in the framework unchanged. | It takes `IDbConnection`, so the framework stays free of connection-source knowledge. |
| One connection per query, not one per test. | A per-test connection is held during slow UI steps and wastes the pool. |

### Why dispose is mandatory

`PostgreSqlConnectionPool.GetConnection` creates a new connection from `NpgsqlDataSource` and opens it. Dapper closes only connections that it opened itself. If a repository passes `GetConnection()` straight to `DapperActions` without `using`, the connection stays open. `MaxPoolSize` is 20, so the pool runs out during a long run.

### Parallel safety

- `NpgsqlDataSource` is thread-safe. The pool keeps data sources in a `ConcurrentDictionary`.
- Each call gets its own `NpgsqlConnection`. Nothing is shared between tests or threads.
- Pool demand equals the number of queries running at the same moment, not the number of tests.
- Limits: steps in one test can run on different connections. `BEGIN`, temp tables, `SET`, and advisory locks do not carry across calls. For atomic multi-statement work, add a separate method that takes a connection or runs a delegate inside one connection.
- Parallel tests can still collide on shared rows. Use unique data per test and clean up only what the test created.

## Steps

### 1. Add the helper

Path: `CsharpTestAutomation.Tests/Database/<App>/Queries/<App>Database.cs`

```csharp
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

internal static class NetBoxDatabase
{
    private const string DatabaseName = "netbox";

    public static T Run<T>(Func<NpgsqlConnection, T> query)
    {
        using NpgsqlConnection connection = PostgreSqlConnectionPool.Instance.GetConnection(DatabaseName);
        return query(connection);
    }
}
```

Replace `NetBox` and `"netbox"` with the application name and the `DbName` from `DbSettings.Database[]` in `appsettings.json`.

### 2. Change each repository

- Remove the `NpgsqlConnection connection` parameter from every public method.
- Remove `using Npgsql;`.
- Wrap the `DapperActions` call in `<App>Database.Run(...)`.

```csharp
public static SiteRowDto? GetBySlug(string slug)
{
    const string sql = """
        SELECT id, name, slug, status
        FROM dcim_site
        WHERE slug = @Slug
        """;

    return NetBoxDatabase.Run(connection =>
        DapperActions.Query<SiteRowDto>(connection, sql, new DynamicParameters(new { Slug = slug })));
}
```

For non-query statements use the same shape: `Run(connection => DapperActions.Execute(connection, sql, parameters))`.

Dispose is safe here because `DapperActions.Query<T>` uses `QueryFirstOrDefault` and `QueryAll<T>` materializes a list. The result is fully read before the connection is disposed. Do not return a lazy `IEnumerable` from inside `Run`.

### 3. Update the callers

In every test:

- Delete `using NpgsqlConnection connection = PostgreSqlConnectionPool.Instance.GetConnection("...");`.
- Remove the `connection` argument from the repository call.
- Remove `using Npgsql;` and `using <Root>.Tests.Database;` if nothing else in the file uses them.

Find the leftovers:

```pwsh
rg "PostgreSqlConnectionPool|NpgsqlConnection|GetBySlug\(connection" CsharpTestAutomation.Tests/Tests
```

Only `PostgreSqlConnectionPool.cs`, the `<App>Database` helper, and the global setup fixture (`OpenConnections` and `CloseConnections`) should still reference the pool.

### 4. Update the instruction files

In `.agents/rules/test-automation.md`, replace "Test Data" rule 5 with:

```markdown
5. Never open a connection or embed a connection string in a test. Call the
   `Database/<App>/Queries` repository methods; they take no connection. Each
   repository acquires a short-lived connection from the shared
   `PostgreSqlConnectionPool` through its `<App>Database.Run(...)` helper, which
   disposes the connection after the query.
```

Update any doc that shows the old call, for example a spec that mentions `PostgreSqlConnectionPool.Instance.GetConnection("netbox")`.

### 5. Validate

```pwsh
dotnet build .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
dotnet format .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj --verify-no-changes --no-restore
```

Then run the tests that use the DB, if the services are available.

## Pitfalls found while applying it

- **Line endings.** The repo uses LF. A new file created on Windows can have CRLF, and `dotnet format` reports `ENDOFLINE` errors. Convert the new file to LF.
- **Using order.** Removing `using Npgsql;` changed the order check in one test file. `using static ...` must come after the normal usings. Run `dotnet format --verify-no-changes` and fix `IMPORTS` errors.
- **Same text twice.** In a test file with several identical `using NpgsqlConnection ...` lines, a single-match edit fails. Use a regex replace for all occurrences.
- **Analyzer rule.** `TreatWarningsAsErrors=true`, so an unused using or a style mismatch fails the build.

## Not covered

- `DapperActions` is synchronous, while `.agents/rules/csharp.md` says all I/O must be asynchronous. This gap exists before and after the change. An async DB layer is a separate change.
- Multi-statement transactions need a dedicated method that keeps one connection open.

## Files changed in this repo (reference)

- Added: `CsharpTestAutomation.Tests/Database/NetBox/Queries/NetBoxDatabase.cs`
- Changed: `SitesDatabaseRepository.cs`, `DevicesDatabaseRepository.cs`, `IpamDatabaseRepository.cs`
- Changed: `Tests/API/SitesApiTests.cs`, `Tests/UI/NetBox/SiteManagementUiTests.cs`
- Changed docs: `.agents/rules/test-automation.md`, `docs/netbox-automation/01-infrastructure-spec.md`
