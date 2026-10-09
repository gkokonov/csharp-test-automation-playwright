using CsharpTestAutomation.Framework.Common;

namespace CsharpTestAutomation.Bdd.Tests.Lifecycle;

/// <summary>One scenario owner: evidence, data cleanup, then resource releases in reverse acquisition order.</summary>
public sealed class ScenarioLifecycle(ScenarioCleanupActions cleanup)
{
    private readonly List<(string Name, Func<Task> Action)> _evidence = [];
    private readonly List<(string Name, Func<Task> Action)> _releases = [];
    private bool _completed;

    public void AddEvidence(string name, Func<Task> capture) => _evidence.Add((name, capture));

    public void AddRelease(string name, Func<Task> release) => _releases.Add((name, release));

    public async Task<AggregateException?> CompleteAsync(Exception? primaryError, Action<string, Exception> report)
    {
        if (_completed)
        {
            return null;
        }

        _completed = true;
        if (primaryError is not null)
        {
            foreach ((string name, Func<Task> capture) in _evidence)
            {
                await AttemptAsync(name, capture, report, null);
            }
        }

        List<Exception> errors = [];
        await AttemptAsync("Site cleanup", () => cleanup.CleanUpAsync(), report, errors);
        for (int index = _releases.Count - 1; index >= 0; index--)
        {
            (string name, Func<Task> release) = _releases[index];
            await AttemptAsync(name, release, report, errors);
        }

        _evidence.Clear();
        _releases.Clear();
        return primaryError is null && errors.Count > 0
            ? new AggregateException("Scenario teardown failed.", errors)
            : null;
    }

    internal static async Task AttemptAsync(string name, Func<Task> action,
        Action<string, Exception> report, List<Exception>? errors)
    {
        try
        {
            await action();
        }
        catch (Exception error)
        {
            errors?.Add(error);
            try
            {
                report(name, error);
            }
            catch (Exception reportingError)
            {
                // Failure reporting cannot prevent the remaining resource releases.
                TestContext.Error.WriteLine($"Could not report {name}: {reportingError}");
            }
        }
    }
}
