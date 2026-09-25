using System.Collections.Concurrent;

namespace CsharpTestAutomation.Tests.Authentication;

public sealed record BootstrapAuthState(string Token, string StorageStateJson, string SessionStorageJson);

public static class BootstrapSession
{
    private static readonly ConcurrentDictionary<string, BootstrapAuthState> s_states = new(StringComparer.OrdinalIgnoreCase);
    private static volatile string? s_defaultKey;

    public static bool HasAny => !s_states.IsEmpty;

    public static BootstrapAuthState Default =>
        s_defaultKey is not null
            ? Get(s_defaultKey)
            : throw new InvalidOperationException("No bootstrapped auth state available. Was GlobalSetupFixture run?");

    public static void Set(string userKey, BootstrapAuthState state)
    {
        s_states[userKey] = state;
        s_defaultKey ??= userKey;
    }

    public static BootstrapAuthState Get(string userKey) =>
        s_states.TryGetValue(userKey, out BootstrapAuthState? state)
            ? state
            : throw new InvalidOperationException($"No bootstrapped auth state for user key '{userKey}'. Was GlobalSetupFixture run?");

    public static void Clear()
    {
        s_states.Clear();
        s_defaultKey = null;
    }
}
