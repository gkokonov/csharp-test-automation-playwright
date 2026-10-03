using Allure.NUnit;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Framework.Test.Tests.Common.Utilities;

[TestFixture]
[AllureNUnit]
[Category("TestIdentifierTests")]
public class TestIdentifierTests
{
    [Test]
    public void Verify_TestIdContainsMethodName_When_CalledWithinTest()
    {
        var testId = TestIdentifier.GetTestId();

        testId.Should().NotBeNullOrEmpty();
        testId.Should().Contain(nameof(Verify_TestIdContainsMethodName_When_CalledWithinTest));
    }

    [Test]
    public void Verify_TestIdRemainsStable_When_CalledMultipleTimes()
    {
        var first = TestIdentifier.GetTestId();
        var second = TestIdentifier.GetTestId();

        second.Should().Be(first);
    }

    [Test]
    public void Verify_EquivalentTestIdGenerated_When_CacheIsCleared()
    {
        var before = TestIdentifier.GetTestId();

        TestIdentifier.ClearCache();
        var after = TestIdentifier.GetTestId();

        after.Should().Be(before);
    }

    [Test]
    public void Verify_InvalidOperationExceptionThrown_When_CalledOutsideTestContext()
    {
        Exception? captured = null;

        // Suppress AsyncLocal flow so the worker thread does NOT inherit NUnit's real
        // TestExecutionContext. NUnit then supplies an AdhocContext, reproducing the
        // "called outside any test/suite" scenario (e.g. a static constructor that spins up
        // Playwright before any test executes).
        using (ExecutionContext.SuppressFlow())
        {
            var worker = new Thread(() =>
            {
                try
                {
                    _ = TestIdentifier.GetTestId();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });

            worker.Start();
            worker.Join();
        }

        captured.Should().BeOfType<InvalidOperationException>();
        captured!.Message.Should().Contain("outside of an NUnit test or suite context");
    }
}

[TestFixture]
[AllureNUnit]
[Category("TestIdentifierTests")]
public class TestIdentifierSuiteContextTests
{
    private static string s_oneTimeSetUpTestId;
    private static Exception? s_oneTimeSetUpException;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // A [SetUpFixture] / [OneTimeSetUp] is the supported place for prerequisite data
        // preparation. MethodName is null here, but a stable suite ID exists, so GetTestId
        // must produce a deterministic suite-scoped identifier instead of throwing.
        try
        {
            s_oneTimeSetUpTestId = TestIdentifier.GetTestId();
        }
        catch (Exception ex)
        {
            s_oneTimeSetUpException = ex;
        }
    }

    [Test]
    public void Verify_SuiteIdGenerated_When_CalledFromOneTimeSetUp()
    {
        s_oneTimeSetUpException.Should().BeNull();
        s_oneTimeSetUpTestId.Should().NotBeNullOrEmpty();
        s_oneTimeSetUpTestId.Should().Contain("Suite");
    }
}
