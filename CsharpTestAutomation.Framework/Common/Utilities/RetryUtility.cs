using CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;
using NLog;
using Polly;
using Polly.Retry;

namespace CsharpTestAutomation.Framework.Common.Utilities;

/// <summary>
/// Provides Polly-based retry helpers for async operations that may transiently fail or return a
/// result that is not ready yet. All retry events are logged at Info level via NLog.
/// </summary>
public static class PollyUtility
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    /// <summary>Runs an asynchronous action and retries matching exceptions.</summary>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="retryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryEveryNSeconds">The delay between attempts.</param>
    /// <typeparam name="TException">The exception type that triggers a retry.</typeparam>
    public static async Task WaitForAction<TException>(Func<Task> action, int retryCount = 3, int retryEveryNSeconds = 1)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);
        ArgumentOutOfRangeException.ThrowIfNegative(retryEveryNSeconds);

        AsyncRetryPolicy retry = RetryDefinition<TException>(retryCount, retryEveryNSeconds);
        await retry.ExecuteAsync(action).ConfigureAwait(false);
    }

    /// <summary>Runs an asynchronous operation and captures the final result after matching exceptions are retried.</summary>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="retryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryEveryNSeconds">The delay between attempts.</param>
    /// <typeparam name="TResult">The operation result type.</typeparam>
    /// <typeparam name="TException">The exception type that triggers a retry.</typeparam>
    public static async Task<PolicyResult<TResult>> WaitForActionWithResult<TResult, TException>(Func<Task<TResult>> action, int retryCount = 3, int retryEveryNSeconds = 1)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);
        ArgumentOutOfRangeException.ThrowIfNegative(retryEveryNSeconds);

        AsyncRetryPolicy retry = RetryDefinition<TException>(retryCount, retryEveryNSeconds);
        return await retry.ExecuteAndCaptureAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs <paramref name="action"/> and retries while <paramref name="retryWhile"/> returns
    /// <see langword="true"/> for its result. Exceptions are not retried.
    /// </summary>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="retryWhile">Returns <see langword="true"/> while the result is not ready.</param>
    /// <param name="retryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryEveryNSeconds">The base delay between attempts.</param>
    /// <param name="cancellationToken">Token used to cancel the operation and delays.</param>
    /// <returns>The first result for which <paramref name="retryWhile"/> returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="retryCount"/> is less than 1, or <paramref name="retryEveryNSeconds"/> is negative.</exception>
    /// <exception cref="RetryException">The result still matches <paramref name="retryWhile"/> after all retries.</exception>
    public static async Task<TResult> WaitForResult<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        Func<TResult, bool> retryWhile,
        int retryCount = 3,
        int retryEveryNSeconds = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(retryWhile);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(retryCount);
        ArgumentOutOfRangeException.ThrowIfNegative(retryEveryNSeconds);

        var resultMatched = false;
        ResiliencePipeline<TResult> pipeline = new ResiliencePipelineBuilder<TResult>()
            .AddRetry(new RetryStrategyOptions<TResult> {
                ShouldHandle = new PredicateBuilder<TResult>()
                    .HandleResult(result => resultMatched = retryWhile(result)),
                MaxRetryAttempts = retryCount,
                Delay = TimeSpan.FromSeconds(retryEveryNSeconds),
                BackoffType = DelayBackoffType.Constant,
                UseJitter = true,
                OnRetry = args =>
                {
                    s_logger.Info($"Result matched retry condition; retry {args.AttemptNumber + 1} of {retryCount} in {args.RetryDelay.TotalMilliseconds:F0}ms.");
                    return default;
                }
            })
            .Build();

        TResult result = await pipeline.ExecuteAsync(
            async (CancellationToken token) => await action(token).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);

        return resultMatched
            ? throw new RetryException($"Result still matched the retry condition after {retryCount} retries.")
            : result;
    }

    private static AsyncRetryPolicy RetryDefinition<TException>(int retryCount, int retryEveryNSeconds)
        where TException : Exception
    {
        return Policy
            .Handle<TException>()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(retryEveryNSeconds), OnRetry);
    }

    private static void OnRetry(Exception exception, TimeSpan timeSpan, int retry, Context context) => s_logger.Info($"Exception thrown for retry count of '{retry}' : {exception}");
}
