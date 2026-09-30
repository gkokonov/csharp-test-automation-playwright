namespace CsharpTestAutomation.Framework.Common;

/// <summary>
/// Collects test data cleanup actions and runs them in LIFO order in an AfterScenario or TearDown hook.
/// Synchronous and asynchronous actions share one ordered stack, so the most recently registered action
/// always runs first, whatever its kind. All members are thread-safe.
/// </summary>
public class ScenarioCleanupActions
{
    private static readonly NLog.Logger s_log = NLog.LogManager.GetCurrentClassLogger();

    private readonly Lock _lock = new();
    private readonly List<CleanUpEntry> _actions = [];

    /// <summary>Adds a synchronous cleanup action to the stack.</summary>
    /// <param name="cleanUpAction">Action to add.</param>
    public void AddCleanUpAction(Action cleanUpAction)
    {
        ArgumentNullException.ThrowIfNull(cleanUpAction);
        Push(new CleanUpEntry(cleanUpAction, () =>
        {
            cleanUpAction();
            return Task.CompletedTask;
        }, IsAsync: false));
    }

    /// <summary>Adds an asynchronous cleanup action to the stack.</summary>
    /// <param name="cleanUpAction">Async action to add.</param>
    public void AddCleanUpAction(Func<Task> cleanUpAction)
    {
        ArgumentNullException.ThrowIfNull(cleanUpAction);
        Push(new CleanUpEntry(cleanUpAction, cleanUpAction, IsAsync: true));
    }

    /// <summary>Removes the most recently added synchronous cleanup action, if any.</summary>
    public void RemoveLastCleanUpAction() => RemoveLast(isAsync: false);

    /// <summary>Removes the most recently added asynchronous cleanup action, if any.</summary>
    public void RemoveLastAsyncCleanUpAction() => RemoveLast(isAsync: true);

    /// <summary>Removes all cleanup actions from the stack.</summary>
    public void RemoveAllCleanUpActions()
    {
        lock (_lock)
        {
            _actions.Clear();
        }
    }

    /// <summary>
    /// Executes all cleanup actions in LIFO order. This method bridges asynchronous cleanup for
    /// synchronous callers.
    /// </summary>
    /// <param name="failOnException">Fail the test on cleanup error, default is true.</param>
    /// <exception cref="AggregateException">Thrown when cleanup fails and failOnException is true.</exception>
    public void CleanUp(bool failOnException = true) => CleanUpAsync(failOnException).GetAwaiter().GetResult();

    /// <summary>Executes all cleanup actions in LIFO order asynchronously.</summary>
    /// <param name="failOnException">Fail the test on cleanup error, default is true.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">Thrown when cleanup fails and failOnException is true.</exception>
    public async Task CleanUpAsync(bool failOnException = true)
    {
        CleanUpEntry[] actions;
        lock (_lock)
        {
            actions = [.. _actions];
            _actions.Clear();
        }

        List<Exception>? exceptions = null;
        for (var i = actions.Length - 1; i >= 0; i--)
        {
            try
            {
                s_log.Debug($"Executing Cleanup of Method: {actions[i].Original.Method}");
                await actions[i].Run().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
                s_log.Error(ex);
            }
        }

        if (exceptions is not null && failOnException)
        {
            throw new AggregateException("Cleanup error occurred!", exceptions);
        }
    }

    private void Push(CleanUpEntry entry)
    {
        lock (_lock)
        {
            _actions.Add(entry);
        }
    }

    private void RemoveLast(bool isAsync)
    {
        lock (_lock)
        {
            var index = _actions.FindLastIndex(entry => entry.IsAsync == isAsync);
            if (index >= 0)
            {
                _actions.RemoveAt(index);
            }
        }
    }

    private readonly record struct CleanUpEntry(Delegate Original, Func<Task> Run, bool IsAsync);
}
