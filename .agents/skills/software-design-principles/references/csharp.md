# C# and .NET Guide

Use modern C# idioms, but follow the target repository's language version, nullable settings, analyzers, and dependency-injection conventions.

This guide covers production code. For tests, suites, and test frameworks read [csharp-testing.md](csharp-testing.md) instead.

## Contents

- [Design choices](#design-choices)
- [Registration example](#complete-example-separating-policy-persistence-and-notification)
- [Review checks](#review-checks)

## Design choices

- Use a `record` or `readonly record struct` for value-like data when value equality and immutability match the domain. Do not convert identity-bearing, mutable entities merely for terseness.
- Use an `interface` for a capability consumed through multiple implementations, an external/volatile boundary, or a useful test seam. Do not create `IFoo` for every sealed helper.
- Use an `abstract class` when implementations genuinely share a stable contract plus protected implementation. Keep inheritance shallow and test every subtype against the base contract.
- A delegate such as `Func<T, TResult>` or a named delegate is often the simplest DIP/strategy boundary for one operation.
- Prefer constructor injection for required dependencies. Avoid service location and public settable dependencies. Inject `TimeProvider`, random/ID sources, or I/O boundaries when deterministic tests need them.
- Keep LINQ when it expresses a recognizable transformation; choose explicit control flow when it makes branching, early exits, allocations, or error handling clearer.
- Narrow consumer contracts around application needs rather than mirroring a provider SDK or repository implementation.

## Complete example: separating policy, persistence, and notification

The initial service has several independent reasons to change and constructs its own details:

<!-- example: registration-before | verify: compile, run -->
```csharp
using System;
using System.Collections.Generic;

public sealed record Registration(string Email);

public sealed class RegistrationService
{
    private readonly List<string> _emails = new();

    public void Register(Registration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Email) ||
            !registration.Email.Contains('@'))
        {
            throw new ArgumentException("A valid email is required.");
        }

        _emails.Add(registration.Email);
        Console.WriteLine($"Welcome {registration.Email}");
    }
}

public static class Program
{
    public static void Main() =>
        new RegistrationService().Register(new Registration("user@example.com"));
}
```

When persistence and delivery demonstrably change independently, split those two boundaries while keeping orchestration cohesive:

<!-- example: registration-after | verify: compile, run -->
```csharp
using System;
using System.Collections.Generic;

public sealed record Registration(string Email);

public interface IRegistrationRepository
{
    void Add(Registration registration);
}

public interface IWelcomeNotifier
{
    void Send(Registration registration);
}

public sealed class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly List<Registration> _items = new();
    public void Add(Registration registration) => _items.Add(registration);
}

public sealed class ConsoleWelcomeNotifier : IWelcomeNotifier
{
    public void Send(Registration registration) =>
        Console.WriteLine($"Welcome {registration.Email}");
}

public sealed class RegistrationService
{
    private readonly IRegistrationRepository _repository;
    private readonly IWelcomeNotifier _notifier;

    public RegistrationService(
        IRegistrationRepository repository,
        IWelcomeNotifier notifier)
    {
        _repository = repository;
        _notifier = notifier;
    }

    public void Register(Registration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Email) ||
            !registration.Email.Contains('@'))
        {
            throw new ArgumentException("A valid email is required.");
        }

        _repository.Add(registration);
        _notifier.Send(registration);
    }
}

public static class Program
{
    public static void Main()
    {
        var service = new RegistrationService(
            new InMemoryRegistrationRepository(),
            new ConsoleWelcomeNotifier());
        service.Register(new Registration("user@example.com"));
    }
}
```

**Why it is justified:** The use case retains one coherent workflow. Interfaces appear only at boundaries with plausible alternate implementations or test doubles. Validation appears identically in both versions because this refactor deliberately leaves it untouched; it stays local until it becomes shared domain knowledge, and extracting it now would be speculative. Persistence and delivery were separated because they have demonstrated independent change and need test doubles; validation had neither.

## Review checks

- Are `NotSupportedException` overrides exposing a false base contract?
- Are interfaces consumer-shaped, or copied one-to-one from implementations?
- Does a service constructor reveal required dependencies, or hide them behind `IServiceProvider`?
- Would a delegate be clearer than a one-method interface?
- Do records/value objects preserve domain identity and serialization compatibility?
- Are extension methods clarifying stable operations or hiding dependency navigation?
- Can all implementations pass the same contract test fixture?
