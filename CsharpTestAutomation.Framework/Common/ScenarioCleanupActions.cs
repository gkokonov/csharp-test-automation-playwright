namespace CsharpTestAutomation.Framework.Common;

/// <summary>
/// This class represents a Stack of actions that will be executed one by one (LIFO) in
/// AfterScenario hook. It can be used for Test Data cleanup actions that need to be executed
/// after a scenario. Supports both synchronous and asynchronous cleanup operations.
/// </summary>
public class ScenarioCleanupActions
{
    private static readonly NLog.Logger s_log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Synchronous CleanUpActions stack.
    /// </summary>
    public Stack<Action> CleanUpActions { get; private set; }

    /// <summary>
    /// Asynchronous CleanUpActions stack.
    /// </summary>
    public Stack<Func<Task>> AsyncCleanUpActions { get; private set; }

    /// <summary>
    /// Adds new synchronous clean-up action to the stack.
    /// </summary>
    /// <param name="cleanUpAction">Action to add</param>
    public void AddCleanUpAction(Action cleanUpAction)
    {
        CleanUpActions ??= new Stack<Action>();
        CleanUpActions.Push(cleanUpAction);
    }

    /// <summary>
    /// Adds new asynchronous clean-up action to the stack.
    /// </summary>
    /// <param name="cleanUpAction">Async action to add</param>
    public void AddCleanUpAction(Func<Task> cleanUpAction)
    {
        AsyncCleanUpActions ??= new Stack<Func<Task>>();
        AsyncCleanUpActions.Push(cleanUpAction);
    }

    /// <summary>
    /// Remove last synchronous clean-up action from the stack.
    /// </summary>
    public void RemoveLastCleanUpAction()
    {
        CleanUpActions?.Pop();
    }

    /// <summary>
    /// Remove last asynchronous clean-up action from the stack.
    /// </summary>
    public void RemoveLastAsyncCleanUpAction()
    {
        AsyncCleanUpActions?.Pop();
    }

    /// <summary>
    /// Remove all clean-up actions from the stack.
    /// </summary>
    public void RemoveAllCleanUpActions()
    {
        CleanUpActions?.Clear();
        AsyncCleanUpActions?.Clear();
    }

    /// <summary>
    /// Execute all clean-up actions from the stack in LIFO manner. Usually done in
    /// AfterScenario Hook. This method handles both synchronous and asynchronous cleanup actions.
    /// </summary>
    /// <param name="failOnException">Fail test on clean-up error, default is true</param>
    /// <exception cref="AggregateException">
    /// Throws exception only if failOnException is set to true
    /// </exception>
    public void CleanUp(bool failOnException = true)
    {
        // Execute async cleanup operations synchronously by blocking
        CleanUpAsync(failOnException).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Execute all clean-up actions from the stack in LIFO manner asynchronously. Usually done
    /// in AfterScenario Hook. This method handles both synchronous and asynchronous cleanup actions.
    /// </summary>
    /// <param name="failOnException">Fail test on clean-up error, default is true</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="AggregateException">
    /// Throws exception only if failOnException is set to true
    /// </exception>
    public async Task CleanUpAsync(bool failOnException = true)
    {
        List<Exception> exceptions = null;

        try
        {
            // Process async cleanup actions
            if (AsyncCleanUpActions != null)
            {
                foreach (Func<Task> action in AsyncCleanUpActions)
                {
                    try
                    {
                        s_log.Debug($"Executing Async Cleanup of Method: {action.Method}");
                        await action();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                        s_log.Error(ex);
                    }
                }
            }

            // Process sync cleanup actions
            if (CleanUpActions != null)
            {
                foreach (Action action in CleanUpActions)
                {
                    try
                    {
                        s_log.Debug($"Executing Cleanup of Method: {action.Method}");
                        action();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                        s_log.Error(ex);
                    }
                }
            }
        }
        finally
        {
            // Clear the stacks after execution
            CleanUpActions = null;
            AsyncCleanUpActions = null;
        }

        // Throw aggregated exceptions if required
        if (exceptions != null && failOnException)
        {
            throw new AggregateException("Cleanup error occurred!", exceptions);
        }
    }
}
