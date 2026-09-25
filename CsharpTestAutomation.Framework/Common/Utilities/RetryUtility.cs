using NLog;
using Polly;
using Polly.Retry;

namespace CsharpTestAutomation.Framework.Common.Utilities;

/// <summary>
/// Provides Polly-based retry helpers for executing async operations that may transiently fail.
/// Callers specify the exception type to handle, the maximum retry count, and the fixed delay
/// between attempts. All retry events are logged at Info level via NLog.
/// </summary>
public static class PollyUtility
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public static async Task WaitForAction<TException>(Func<Task> action, int retryCount = 3, int retryEveryNSeconds = 1)
        where TException : Exception
    {
        AsyncRetryPolicy retry = RetryDefinition<TException>(retryCount, retryEveryNSeconds);
        await retry.ExecuteAsync(action).ConfigureAwait(false);
    }

    public static Task<PolicyResult<TResult>> WaitForActionWithResult<TResult, TException>(Func<Task<TResult>> action, int retryCount = 3, int retryEveryNSeconds = 1)
        where TException : Exception
    {
        AsyncRetryPolicy retry = RetryDefinition<TException>(retryCount, retryEveryNSeconds);
        return retry.ExecuteAndCaptureAsync(action);
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
