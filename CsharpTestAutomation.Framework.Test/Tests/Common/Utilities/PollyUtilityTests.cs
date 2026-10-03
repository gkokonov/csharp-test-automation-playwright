using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;
using Polly;

namespace CsharpTestAutomation.Framework.Test.Tests.Common.Utilities;

[TestFixture]
[Category("PollyUtilityTests")]
public class PollyUtilityTests
{
    [Test]
    public async Task Verify_ActionCompletes_When_MatchingExceptionIsRetried()
    {
        var attempts = 0;

        await PollyUtility.WaitForAction<InvalidOperationException>(
            () =>
            {
                if (++attempts < 3)
                {
                    throw new InvalidOperationException();
                }

                return Task.CompletedTask;
            },
            retryEveryNSeconds: 0);

        Assert.That(attempts, Is.EqualTo(3));
    }

    [Test]
    public async Task Verify_FinalResultCaptured_When_MatchingExceptionIsRetried()
    {
        var attempts = 0;

        PolicyResult<int> result = await PollyUtility.WaitForActionWithResult<int, InvalidOperationException>(
            () =>
            {
                if (++attempts < 2)
                {
                    throw new InvalidOperationException();
                }

                return Task.FromResult(attempts);
            },
            retryEveryNSeconds: 0);

        Assert.That(result.Result, Is.EqualTo(2));
        Assert.That(attempts, Is.EqualTo(2));
    }

    [Test]
    public async Task Verify_ThirdResultReturned_When_MatchOccursOnThirdAttempt()
    {
        var attempts = 0;

        var result = await PollyUtility.WaitForResult(
            _ => Task.FromResult(++attempts),
            attempt => attempt < 3,
            retryCount: 3,
            retryEveryNSeconds: 0);

        Assert.That(result, Is.EqualTo(3));
        Assert.That(attempts, Is.EqualTo(3));
    }

    [Test]
    public async Task Verify_RetryExceptionThrown_When_ResultNeverMatches()
    {
        var attempts = 0;

        Task Act() => PollyUtility.WaitForResult(
            _ => Task.FromResult(++attempts),
            _ => true,
            retryCount: 2,
            retryEveryNSeconds: 0);

        Assert.That(async () => await Act(), Throws.TypeOf<RetryException>());
        Assert.That(attempts, Is.EqualTo(3));
    }

    [Test]
    public async Task Verify_ActionExceptionPropagated_When_ActionThrows()
    {
        var attempts = 0;

        Task Act() => PollyUtility.WaitForResult<int>(
            _ =>
            {
                attempts++;
                throw new InvalidOperationException("boom");
            },
            _ => true,
            retryCount: 3,
            retryEveryNSeconds: 0);

        Assert.That(async () => await Act(), Throws.TypeOf<InvalidOperationException>());
        Assert.That(attempts, Is.EqualTo(1));
    }

    [Test]
    public void Verify_ArgumentOutOfRangeExceptionThrown_When_RetryCountIsZero()
    {
        Assert.That(
            () => PollyUtility.WaitForResult(_ => Task.FromResult(1), _ => true, retryCount: 0),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
