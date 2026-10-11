using System.Collections.Concurrent;
using CsharpTestAutomation.Framework.Common;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.UI;

internal sealed class BrowserContextDiagnostics : IDisposable
{
    private readonly IBrowserContext _context;
    private readonly CoreConfiguration _configuration;
    private readonly ConcurrentQueue<string>? _logs;
    private bool _tracingStarted;

    internal BrowserContextDiagnostics(IBrowserContext context, CoreConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        if (configuration.CaptureBrowserLogs)
        {
            _logs = new();
            context.Console += OnConsole;
            context.WebError += OnWebError;
        }
    }

    internal IReadOnlyList<string> GetLogs() => _logs?.ToArray() ?? [];

    internal async Task StartTracingAsync()
    {
        if (_configuration.TraceEnabled || _configuration.TraceOnFailure)
        {
            await _context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true }).ConfigureAwait(false);
            _tracingStarted = true;
        }
    }

    internal async Task<string?> StopTracingAsync(bool testFailed, string testId)
    {
        if (!_tracingStarted)
        {
            return null;
        }

        string? path = null;
        if (_configuration.TraceEnabled || testFailed)
        {
            await Task.Run(() => Directory.CreateDirectory(_configuration.TraceDir)).ConfigureAwait(false);
            string name = string.Join("_", testId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            name = name[..Math.Min(name.Length, 120)];
            path = Path.Combine(_configuration.TraceDir, $"{name}_{Guid.NewGuid():N}.zip");
        }

        await _context.Tracing.StopAsync(new() { Path = path }).ConfigureAwait(false);
        _tracingStarted = false;
        return path;
    }

    internal async Task CloseAsync(string testId)
    {
        List<Exception> errors = [];
        try { await StopTracingAsync(testFailed: false, testId).ConfigureAwait(false); }
        catch (Exception ex) { errors.Add(ex); }
        try { Dispose(); }
        catch (Exception ex) { errors.Add(ex); }
        try { await _context.CloseAsync().ConfigureAwait(false); }
        catch (Exception ex) { errors.Add(ex); }
        if (errors.Count > 0)
        {
            throw new AggregateException($"Browser context release failed for {testId}.", errors);
        }
    }

    public void Dispose()
    {
        if (_logs is not null)
        {
            _context.Console -= OnConsole;
            _context.WebError -= OnWebError;
        }
    }

    private void OnConsole(object? sender, IConsoleMessage message) =>
        _logs?.Enqueue($"[{message.Page?.Url ?? "worker"}] console.{message.Type}: {message.Text}");

    private void OnWebError(object? sender, IWebError error) =>
        _logs?.Enqueue($"[{error.Page?.Url ?? "unknown page"}] JavaScript error: {error.Error}");
}
