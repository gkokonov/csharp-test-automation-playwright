using System.Collections.Concurrent;
using System.Text;

namespace CsharpTestAutomation.Framework.Common.Utilities;

/// <summary>
/// Provides utilities for test identification and tracking
/// </summary>
public static class TestIdentifier
{
    // Thread-safe cache of test IDs
    private static readonly ConcurrentDictionary<string, string> s_testIdCache = new();

    // NUnit supplies an AdhocContext whenever a test context is requested but none is active
    // (for example from a static constructor or a non-NUnit runner). That context exposes a
    // synthetic method with this name, so it is the reliable signal that we are outside any
    // real test or suite execution.
    private const string AdhocTestMethodName = "AdhocTestMethod";

    // Placeholder for the method-name segment when running inside a suite-level context such as
    // [OneTimeSetUp] or a [SetUpFixture], where MethodName is null but a stable suite ID exists.
    private const string SuiteContextSegment = "Suite";

    /// <summary>
    /// Gets a unique ID for the current test that's safe for use in filenames and dictionary
    /// keys. Uses caching to avoid recalculating the same ID multiple times.
    /// </summary>
    /// <returns>A unique, sanitized test identifier</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called outside of any NUnit test or suite context (for example from a static
    /// constructor or a non-NUnit runner), where a stable per-test identity cannot be established.
    /// </exception>
    public static string GetTestId()
    {
        // Read the context once so the cache key and the generated ID can never diverge.
        TestContextSnapshot snapshot = CaptureContext();

        EnsureWithinTestContext(snapshot);

        // Return cached value if available, otherwise compute and cache.
        return s_testIdCache.GetOrAdd(CreateTestContextKey(snapshot), _ => GenerateTestId(snapshot));
    }

    /// <summary>
    /// Clears the test ID cache
    /// </summary>
    public static void ClearCache() => s_testIdCache.Clear();

    /// <summary>
    /// Captures the identifying values of the current NUnit context in a single read so that the
    /// cache key and the generated ID are always built from a consistent snapshot.
    /// </summary>
    private static TestContextSnapshot CaptureContext()
    {
        TestContext context = TestContext.CurrentContext;

        return new TestContextSnapshot(
            context.Test.ID,
            context.Test.DisplayName ?? "DisplayNameNotAvailable",
            context.Test.MethodName ?? "MethodNameNotAvailable",
            context.CurrentRepeatCount);
    }

    /// <summary>
    /// Guards against requesting a test ID when there is no real NUnit test or suite context
    /// backing the call, which would otherwise produce a non-unique sentinel identifier.
    /// </summary>
    private static void EnsureWithinTestContext(TestContextSnapshot snapshot)
    {
        var isAdhocContext = string.Equals(snapshot.MethodName, AdhocTestMethodName, StringComparison.Ordinal);

        if (isAdhocContext || string.IsNullOrEmpty(snapshot.Id))
        {
            throw new InvalidOperationException(
                "TestIdentifier.GetTestId() was called outside of an NUnit test or suite context. " +
                "Request browser/reporting resources only from within a [Test], [SetUp]/[TearDown], " +
                "[OneTimeSetUp]/[OneTimeTearDown], or [SetUpFixture]. For prerequisite data preparation " +
                "that must run before any test, use a [SetUpFixture] so a stable suite context exists.");
        }
    }

    /// <summary>
    /// Creates a unique key for the current test context
    /// </summary>
    /// <returns>A unique key based on test context</returns>
    private static string CreateTestContextKey(TestContextSnapshot snapshot) => $"{snapshot.Id}|{snapshot.DisplayName}|{ResolveMethodSegment(snapshot.MethodName)}|{snapshot.RepeatCount}";

    /// <summary>
    /// Generates a test ID from the current test context
    /// </summary>
    /// <returns>A formatted test ID</returns>
    private static string GenerateTestId(TestContextSnapshot snapshot)
    {
        StringBuilder testIdBuilder = new StringBuilder(snapshot.DisplayName)
            .Append('_')
            .Append(ResolveMethodSegment(snapshot.MethodName))
            .Append('_')
            .Append(snapshot.Id);

        // Add current test iteration for retried tests
        if (snapshot.RepeatCount > 0)
        {
            testIdBuilder.Append("_Retry")
                .Append(snapshot.RepeatCount);
        }

        return testIdBuilder.ToString();
    }

    /// <summary>
    /// Resolves the method-name segment, substituting a stable placeholder when running in a
    /// suite-level context (such as [OneTimeSetUp] or a [SetUpFixture]) where MethodName is null.
    /// </summary>
    private static string ResolveMethodSegment(string methodName) => string.IsNullOrEmpty(methodName) ? SuiteContextSegment : methodName;

    /// <summary>
    /// Immutable snapshot of the NUnit context values used to identify a test.
    /// </summary>
    private readonly record struct TestContextSnapshot(string Id, string DisplayName, string MethodName, int RepeatCount);
}
