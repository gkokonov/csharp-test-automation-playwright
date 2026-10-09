namespace CsharpTestAutomation.Bdd.Tests.Authentication;

/// <summary>Publishes only a successful run-owned snapshot. Later scenarios can retry failed setup.</summary>
public sealed class AuthenticationSnapshot(string path) : IAsyncDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _ready;
    private bool _attempted;

    public async Task<string> GetPathAsync(Func<string, Task> initialize)
    {
        await _initializationLock.WaitAsync();
        try
        {
            if (!_ready)
            {
                _attempted = true;
                try
                {
                    await initialize(path);
                    _ready = true;
                }
                catch (Exception initializationError)
                {
                    try
                    {
                        await RemoveFileAsync();
                    }
                    catch (Exception removalError)
                    {
                        throw new AggregateException("Authentication setup and incomplete-file removal failed.",
                            initializationError, removalError);
                    }

                    throw;
                }
            }

            return path;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_attempted)
            {
                await RemoveFileAsync();
            }
        }
        finally
        {
            _initializationLock.Dispose();
        }
    }

    private Task RemoveFileAsync() => Task.Run(() => File.Delete(path));
}
