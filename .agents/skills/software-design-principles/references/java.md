# Java Guide

Follow the repository's supported Java version, module/build system, nullness conventions, and framework practices. Prefer standard Java constructs in explanations unless the task names a framework.

This guide covers production code. For tests, suites, and test frameworks read [java-testing.md](java-testing.md) instead.

## Contents

- [Design choices](#design-choices)
- [Capability example](#complete-example-model-capabilities-instead-of-a-false-bird-hierarchy)
- [Review checks](#review-checks)

## Design choices

- Use an `interface` for a real role or capability. Interface default methods should preserve the contract for every implementation; do not use them to force optional behaviour.
- Use an abstract class only for a genuine, stable subtype family with shared implementation. Prefer package-private helpers or composition for code reuse that is not an “is-a” relationship.
- Use constructor injection for required collaborators. A `Supplier<T>`, `Function<T,R>`, or other functional interface can be a smaller strategy boundary than a custom interface.
- Use `record` for transparent, immutable data carriers when the supported Java version and domain semantics permit it.
- Use `final` to communicate a deliberately closed implementation. Use sealed types when the variant set is intentionally closed and exhaustive handling is valuable; do not use them for a supposedly open plugin boundary.
- Generics express reusable type relationships, not speculative generality. Introduce type parameters only when multiple real types share the same algorithm and contract.
- Checked and unchecked exceptions are part of behavioural substitutability. An implementation must not surprise callers with failure modes that contradict the advertised contract.

## Complete example: model capabilities instead of a false bird hierarchy

This hierarchy compiles, but `Penguin` cannot honour the base promise:

<!-- example: birds-before | verify: compile, run -->
```java
public final class BirdsBefore {
    interface Bird {
        void fly();
        void layEgg();
    }

    static final class Eagle implements Bird {
        public void fly() { System.out.println("Eagle flies"); }
        public void layEgg() { System.out.println("Eagle lays an egg"); }
    }

    static final class Penguin implements Bird {
        public void fly() {
            throw new UnsupportedOperationException("Penguins cannot fly");
        }
        public void layEgg() { System.out.println("Penguin lays an egg"); }
    }

    static void migrate(Bird bird) {
        bird.fly();
    }

    public static void main(String[] args) {
        migrate(new Eagle());
    }
}
```

Separate universal bird behaviour from the flying capability:

<!-- example: birds-after | verify: compile, run -->
```java
public final class BirdsAfter {
    interface Bird {
        void layEgg();
    }

    interface Flying {
        void fly();
    }

    static final class Eagle implements Bird, Flying {
        public void fly() { System.out.println("Eagle flies"); }
        public void layEgg() { System.out.println("Eagle lays an egg"); }
    }

    static final class Penguin implements Bird {
        public void layEgg() { System.out.println("Penguin lays an egg"); }
    }

    static void migrate(Flying animal) {
        animal.fly();
    }

    public static void main(String[] args) {
        Bird penguin = new Penguin();
        penguin.layEgg();
        migrate(new Eagle());
    }
}
```

**Why it is justified:** No implementation advertises an operation it cannot perform. `Eagle` composes two capability contracts through Java's multiple-interface implementation; this is not multiple class inheritance. Clients request exactly the capability they need.

## Review checks

- Do implementations strengthen input requirements or weaken return/error guarantees?
- Are `instanceof` branches compensating for an invalid base contract?
- Are broad service interfaces forcing no-op or unsupported methods?
- Is an interface a policy-owned boundary or only a mirror of one concrete class?
- Would a functional interface avoid an unnecessary type hierarchy?
- Are mutable base-class fields coupling subclasses to representation details?
- Does a sealed hierarchy represent a deliberately closed variant set, or block expected extension?
