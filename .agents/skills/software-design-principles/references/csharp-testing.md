# C# Test Automation Guide

Cross-language principles live in [Test Automation Design](test-automation.md); production-code design for C# lives in [csharp.md](csharp.md). This guide assumes **NUnit**: the .NET-specific guidance comes first, then the NUnit mechanics that genuinely alter a design, then a mapping table for xUnit and MSTest.

## Contents

- [Design choices](#design-choices)
- [NUnit lifecycle and runner limits](#nunit-specifics-that-change-the-design)
- [Other .NET runners](#mapping-to-other-net-runners)
- [Fixture example](#illustrative-example-shared-fixture-state-to-per-test-composition)
- [Review checks](#review-checks)

## Design choices

- Unlike xUnit, NUnit has no fixture-injection mechanism, so composition is plain object construction: build collaborators—browser session, API client, data builder—in per-test setup and hold them in fields. No container is required, and a base class is not the alternative.
- Prefer typed domain results at API client boundaries when transport details are not part of the behavior under test. When tests must assert status, headers, or other transport metadata, expose the response information needed for those assertions. Follow the repository's API testing guidance for the concrete return type.
- Reuse one `RestClient` or `HttpClient` per base address—both are thread-safe and pool connections—but never mutate default headers per test. Shared defaults plus parallel tests means one test overwrites another's auth. Pass per-request state explicitly.
- Use a `record` for test data and `with` expressions to override only the fields under test; the overrides then document the test's intent.
- Inject `TimeProvider`, random sources, and identifier generators wherever determinism matters.
- Stay asynchronous end to end: `[Test] public async Task`, never `async void`, and no `.Result` or `.Wait()`. Blocking on a test host's synchronization context deadlocks, and it wraps failures in `AggregateException`, which obscures the real assertion message.

## NUnit specifics that change the design

By default NUnit uses `LifeCycle.SingleInstance`: one instance of the test class serves every test in it, so instance fields can be shared between tests. The [NUnit lifecycle documentation](https://docs.nunit.org/articles/nunit/writing-tests/attributes/fixturelifecycle.html) defines the instance and one-time setup rules below. Check the runner integration as well as NUnit before choosing a parallel scope.

- With fixture parallelism and sequential methods inside each fixture, instance fields do not race between those methods, but leaked state can still cause order dependencies. In a plain NUnit fixture, `ParallelScope.All` or `Children` can run methods against the same instance and create races.
- For a suite that parallelises test methods, apply `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`—at assembly level as a default, overridden per class where needed. The constructor then runs before each test and `IDisposable` fixtures are disposed after each test.
- Under `InstancePerTestCase`, `[OneTimeSetUp]` and `[OneTimeTearDown]` must be static so they cannot read fields reset per test. NUnit enforces this runner constraint; it is not a C# compiler check.
- Anything built in `[OneTimeSetUp]` is shared by every concurrently running test in that fixture. It must be immutable or explicitly synchronized.

**Playwright NUnit integration:** The provided `PageTest`/`ContextTest` base classes support `ParallelScope.Self`, with fixtures running in parallel and methods within a fixture sequentially. `InstancePerTestCase` does not make `ParallelScope.All` supported by these base classes. Use the [Playwright .NET runner documentation](https://playwright.dev/dotnet/docs/test-runners) for the installed integration version. Method concurrency with manually composed browser lifetimes is a different design that needs its own verification.

**Inheritance behaves worse than people assume, which is itself an argument for composition.**

- A base-class `[OneTimeSetUp]` runs once *per derived fixture*, not once overall. Suites are routinely built on the belief that it runs once globally. For genuinely-once work use a `[SetUpFixture]` with `[OneTimeSetUp]` for that namespace, or static initialization.
- Base-class `[SetUp]` runs before derived-class `[SetUp]`, and multiple `[SetUp]` methods declared on the same class have **no guaranteed relative order**. A setup path whose correctness depends on ordering is already fragile; composing explicit collaborators removes the ordering question entirely.
- A shallow framework base class that only wires lifecycle—`PageTest` from `Microsoft.Playwright.NUnit`, which supplies a per-test `Page` and `Expect`—is the acceptable case described in [Fixtures, hooks, and base classes](test-automation.md#fixtures-hooks-and-base-classes). The defect is accumulating *your* concerns on top of it.

**Parallelism, assertions, and diagnostics.**

- `[Parallelizable]` defaults to `ParallelScope.Self`. `Self` is the only value valid on a test method and has no effect on an assembly; use `Children` or `Fixtures` at assembly level. `[LevelOfParallelism(n)]` is assembly-only.
- `[Order]` makes tests order-dependent by construction. Reach for it only when a suite genuinely models a sequence, and treat it as a reported cost.
- `[Retry(n)]` is subject to the flaky-test rule in `SKILL.md`. Verify how the installed runner reports individual attempts before using teardown state to label retry artifacts.
- Use the repository's assertion library and the NUnit API available in its version. In explanations, `Assert.That(actual, Is.EqualTo(expected))` makes the expectation explicit. Check migration guidance before changing assertion namespaces or overloads.
- Use supported multiple-assertion scopes when several assertions describe one outcome. NUnit assertion aggregation does not automatically collect Playwright assertion exceptions; follow the repository's guidance on scope boundaries.
- For time limits, verify the runner's cancellation API and pass the supplied cancellation token into cooperating operations. An attribute alone does not cancel arbitrary work.
- Prefer `[TestCase]` and `[TestCaseSource]` with `TestCaseData(...).SetName(...)` over a loop inside one test: each case is reported and named individually, so a failure identifies the input.
- For failure artifacts, check `TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed` in `[TearDown]` and attach with `TestContext.AddTestAttachment(path, description)`. Call it from `[TearDown]` or the test body—inside `[OneTimeSetUp]`/`[OneTimeTearDown]` the context refers to the fixture, not a test. `TestContext.CurrentContext.Test.Name` and `.WorkDirectory` give per-test artifact names and a place to put them.
- `[Ignore]` requires a reason in NUnit 4, so every skip carries its justification. Use `[Category]` for selection rather than commenting tests out.

## Mapping to other .NET runners

Only the instance-per-test row changes a design; the rest is syntax.

| Concern | NUnit | xUnit | MSTest |
| --- | --- | --- | --- |
| Per-test setup | `[SetUp]` | constructor | `[TestInitialize]` |
| Per-test teardown | `[TearDown]` | `IDisposable` / `IAsyncLifetime` | `[TestCleanup]` |
| Once per class | `[OneTimeSetUp]` | `IClassFixture<T>` | `[ClassInitialize]` |
| Once for a wider scope | `[SetUpFixture]` + `[OneTimeSetUp]` | `ICollectionFixture<T>` | `[AssemblyInitialize]` |
| New instance per test | opt in with `[FixtureLifeCycle]` | default | default |
| Inline data | `[TestCase]` | `[InlineData]` | `[DataRow]` |
| External data | `[TestCaseSource]` | `[MemberData]` / `[ClassData]` | `[DynamicData]` |
| Parallel opt-in | `[Parallelizable]`, `[LevelOfParallelism]` | assembly config and collections | runsettings |

## Illustrative example: shared fixture state to per-test composition

The starting point relies on `SingleInstance` fields while parallelising test methods, mutates client defaults per test, sleeps, and puts selectors in the spec:

<!-- example: nunit-before | verify: none (requires NUnit, Playwright, RestSharp) -->
```csharp
public abstract class TestBase
{
    protected static IPage Page;
    protected static RestClient Api;
    protected string AuthToken;

    [OneTimeSetUp]
    public async Task GlobalSetUp()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        Page = await browser.NewPageAsync();
        Api = new RestClient("https://api.example.com");
    }

    [SetUp]
    public async Task SetUp()
    {
        AuthToken = await LogInAsync("standard_user", "secret");
        Api.AddDefaultHeader("Authorization", $"Bearer {AuthToken}");
        await Page.GotoAsync("/orders");
    }
}

[Parallelizable(ParallelScope.All)]
public sealed class OrderTests : TestBase
{
    [Test]
    public async Task CreatesOrder()
    {
        await Page.FillAsync("#customer", "Ada");
        await Page.ClickAsync(".btn-primary");
        await Page.WaitForTimeoutAsync(2000);
        Assert.That(await Page.TextContentAsync(".toast"), Is.EqualTo("Order created"));
    }
}
```

Take the per-test `Page` from `PageTest`, make the lifecycle instance-per-test so fields are not shared, and compose collaborators instead of inheriting them:

<!-- example: nunit-after | verify: none (requires NUnit, Playwright, RestSharp) -->
```csharp
public sealed record Customer(string Name, string Tier = "standard");

public sealed class OrdersPage
{
    private readonly IPage _page;

    public OrdersPage(IPage page) => _page = page;

    private ILocator CustomerName => _page.GetByLabel("Customer");
    private ILocator Submit =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Create order" });

    public ILocator Confirmation => _page.GetByTestId("order-toast");

    public async Task CreateOrderForAsync(Customer customer)
    {
        await CustomerName.FillAsync(customer.Name);
        await Submit.ClickAsync();
    }
}

public sealed class OrdersApi
{
    private readonly RestClient _client;

    public OrdersApi(RestClient client) => _client = client;

    public Task<Order?> CreateAsync(Customer customer, string token) =>
        _client.PostAsync<Order>(new RestRequest("orders", Method.Post)
            .AddHeader("Authorization", $"Bearer {token}")
            .AddJsonBody(customer));
}

[Parallelizable(ParallelScope.Self)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OrderTests : PageTest
{
    private OrdersPage _orders = null!;

    [SetUp]
    public async Task SetUp()
    {
        _orders = new OrdersPage(Page);
        await Page.GotoAsync("/orders");
    }

    [TearDown]
    public async Task AttachFailureArtifacts()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
        {
            return;
        }

        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            $"{TestContext.CurrentContext.Test.Name}.png");
        await Page.ScreenshotAsync(new() { Path = path });
        TestContext.AddTestAttachment(path, "failure screenshot");
    }

    [Test]
    public async Task ShowsConfirmationAfterCreatingAnOrder()
    {
        await _orders.CreateOrderForAsync(new Customer("Ada"));

        await Expect(_orders.Confirmation).ToHaveTextAsync("Order created");
    }
}
```

**Why it is justified:** `PageTest` supplies one page/context per test. `ParallelScope.Self` uses the supported fixture parallelism, and `InstancePerTestCase` prevents instance fields from retaining another test's state. The selector knowledge sits in one page object while the expected text stays in the spec. Auto-retrying `Expect` replaces the fixed sleep. Auth travels per request instead of mutating shared client defaults. `OrdersApi` keeps RestSharp out of the spec without mirroring the SDK. The inherited layer only wires framework lifecycle. This illustrative block requires the surrounding types, imports, packages, and base URL configuration; it is not an executed proof of parallel safety.

## Review checks

- Does the fixture rely on instance fields while parallelising test methods, without `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`?
- Does a `PageTest`/`ContextTest` fixture use unsupported method concurrency such as `ParallelScope.All`, even with `InstancePerTestCase`?
- Is any `IPage`, `IBrowserContext`, browser, or page object held in a `static` field, or built in `[OneTimeSetUp]` and then mutated by tests?
- Does a base-class `[OneTimeSetUp]` assume it runs once overall when it runs once per derived fixture?
- Does the setup path depend on the order of several `[SetUp]` methods on one class, which NUnit does not guarantee?
- Does `TestBase` own anything beyond framework lifecycle, and how deep is the chain?
- Does a spec contain a CSS/XPath string, `WaitForTimeoutAsync`, or `Thread.Sleep`?
- Are default headers or shared client configuration mutated per test?
- Does a spec bypass the repository's API client contract or assert transport details that are not part of its scenario? When transport behavior is under test, assert the relevant response metadata through the project's established API client contract.
- Is `[Retry]` or `[Order]` compensating for a defect or a missing synchronization point?
- Do several assertions describing one outcome use `Assert.Multiple`, or would the first failure hide the rest?
- Are page objects returning `IElementHandle` (stale-prone) where an `ILocator` belongs?
- Is `.Result`, `.Wait()`, or `async void` obscuring failures?
