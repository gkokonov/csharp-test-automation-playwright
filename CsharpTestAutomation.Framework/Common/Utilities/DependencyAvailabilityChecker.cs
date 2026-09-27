using CsharpTestAutomation.Framework.Common.Extensions;
using CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;

namespace CsharpTestAutomation.Framework.Common.Utilities;

/// <summary>
/// Checks the reachability of one or more HTTP dependency endpoints concurrently and reports
/// every failure together, instead of stopping at the first unreachable endpoint.
/// </summary>
public static class DependencyAvailabilityChecker
{
    /// <summary>
    /// Verifies every named dependency URL is reachable. Throws a single
    /// <see cref="DependencyUnavailableException"/> aggregating all failures when one or more checks fail.
    /// </summary>
    public static async Task EnsureAllAvailableAsync(HttpClient client, IReadOnlyCollection<(string Name, string Url)> dependencies, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        IEnumerable<Task<Exception?>> checks = dependencies.Select(async dependency =>
        {
            try
            {
                await client.EnsureAvailableAsync(dependency.Name, dependency.Url, cancellationToken).ConfigureAwait(false);
                return (Exception?)null;
            }
            catch (DependencyUnavailableException ex)
            {
                return ex;
            }
        });

        Exception?[] results = await Task.WhenAll(checks).ConfigureAwait(false);
        Exception[] failures = [.. results.OfType<Exception>()];

        if (failures.Length > 0)
        {
            var message = string.Join(Environment.NewLine, failures.Select(f => f.Message));
            throw new DependencyUnavailableException(message, new AggregateException(failures));
        }
    }
}
