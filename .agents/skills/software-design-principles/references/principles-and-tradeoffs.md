# Principles and Trade-offs

Use this reference to diagnose a design, not as a compliance checklist. A principle matters only when it connects an observed structure to a concrete maintenance, correctness, testability, or delivery consequence.

## Principle index

- [SRP](#single-responsibility-principle-srp)
- [OCP](#open-closed-principle-ocp)
- [LSP](#liskov-substitution-principle-lsp)
- [ISP](#interface-segregation-principle-isp)
- [DIP](#dependency-inversion-principle-dip)
- [DRY](#dry--dont-repeat-yourself)
- [KISS](#kiss--keep-it-simple)
- [YAGNI](#yagni--you-arent-gonna-need-it)
- [Composition](#composition-over-inheritance)
- [Law of Demeter](#law-of-demeter--principle-of-least-knowledge)

## SOLID

### Single Responsibility Principle (SRP)

**Definition and intent:** A module should have one coherent responsibility—more precisely, one primary reason or actor that would require it to change. The goal is high cohesion and changes that remain localized.

**Signals:** A type mixes business policy with persistence, transport, formatting, or notifications; different teams repeatedly edit unrelated portions of the same file; tests need many unrelated fixtures; method groups use disjoint state.

**Ask:** Who requests each change? Which rules evolve together? Which changes come from different actors or policies? Would extracting it create a stable boundary or only shuffle code?

**Useful moves:** Extract a policy or collaborator; separate orchestration from I/O; isolate serialization and transport at boundaries; keep a cohesive façade when it improves use.

**Do not overapply:** One responsibility can require several methods. Tiny classes split around individual operations can scatter a concept, increase navigation, and hide the workflow. Group behaviour that changes for the same reason.

**Interactions:** SRP can expose natural ISP interfaces and DIP boundaries. Excessive splitting conflicts with KISS and can violate YAGNI.

### Open-Closed Principle (OCP)

**Definition and intent:** For a demonstrated variation point, enable new variants through a stable extension mechanism while protecting settled behaviour from repeated edits. OCP is not a ban on modifying existing code.

**Signals:** Every new channel, payment type, rule, or format adds another branch to a central dispatcher; unrelated variants are compiled and deployed together; stable code is repeatedly reopened and regressed.

**Ask:** Is this axis already changing, or merely imagined? What contract is stable? Will extensions be independently owned or deployed? Is a data table, function parameter, union, or strategy simpler than polymorphism?

**Useful moves:** Strategy or function injection; registration table; plugin boundary; discriminated union with exhaustive handling when variants are intentionally closed; composition around a stable core.

**Do not overapply:** The first implementation often should be direct. Editing code to fix defects, improve the model, or add a one-off case is compatible with OCP. A speculative extension framework conflicts with YAGNI.

**Interactions:** DIP can protect a high-level policy at the extension boundary. KISS and YAGNI decide whether the boundary is justified now.

### Liskov Substitution Principle (LSP)

**Definition and intent:** Any implementation advertised as a subtype must be usable wherever the base contract is expected without surprising the client or breaking correctness.

**Contract checks:** A subtype must not require stronger preconditions, promise weaker postconditions, violate invariants, introduce incompatible errors, corrupt expected state transitions, or change essential side effects. “It compiles” is insufficient.

**Signals:** Overrides throw “not supported”; callers inspect subtype identity; a subtype silently ignores a base operation; tests must exclude certain subtypes; mutable inheritance breaks invariants such as rectangle/square setters.

**Ask:** What does the base contract promise? Can every subtype pass the same contract test suite? Does the client need a capability the base type does not universally provide?

**Useful moves:** Split capabilities (`Bird` from `Flying`); replace inheritance with composition; narrow the base contract; use immutable values; create contract tests executed against every implementation.

**Do not overapply:** Different performance characteristics or internal algorithms are acceptable unless the contract promises otherwise. Do not invent an inheritance hierarchy solely to demonstrate polymorphism.

**Interactions:** LSP is the safety condition for inheritance and polymorphism. ISP helps keep contracts honest; composition avoids false subtype claims.

### Interface Segregation Principle (ISP)

**Definition and intent:** A client should depend only on the operations it uses. Model cohesive capabilities rather than forcing implementers to provide unrelated behaviour.

**Signals:** Empty methods, “not supported” exceptions, fat mocks, broad interfaces that change for unrelated consumers, or implementations using only a small subset.

**Ask:** Which methods does each client actually call? Do operations change together? Are splits based on consumer needs and capabilities rather than arbitrary method counts?

**Useful moves:** Role/capability interfaces; separate read from write contracts where consumers differ; accept a function/delegate for a single operation; compose multiple interfaces in implementations that support all capabilities.

**Do not overapply:** A cohesive interface may legitimately contain several operations. Too many one-method interfaces make navigation, dependency wiring, and API discovery harder.

**Interactions:** ISP strengthens LSP by avoiding impossible promises and makes DIP dependencies narrower. Excessive fragmentation conflicts with KISS.

### Dependency Inversion Principle (DIP)

**Definition and intent:** High-level policy and low-level details should depend on a contract owned around the policy boundary; abstractions should not be shaped solely by one concrete detail. Dependency injection is a mechanism, not the principle itself.

**Signals:** Business logic constructs database, HTTP, clock, filesystem, or messaging clients; tests require real infrastructure; changing a provider changes core policy; service locators hide dependencies.

**Ask:** Is the dependency volatile, external, non-deterministic, or expensive? Which operations does policy actually need? Who should own the abstraction? Would a function parameter be clearer than an interface?

**Useful moves:** Constructor injection; ports/adapters; functions, delegates, or suppliers for narrow behaviour; wrap third-party APIs at the boundary; inject clocks and identifiers when determinism matters.

**Do not overapply:** Stable value objects, standard-library utilities, and simple internal helpers do not need interfaces by default. An abstraction identical to a concrete class adds ceremony without inversion.

**Interactions:** DIP supports OCP and test seams. Balance it against KISS and YAGNI.

## DRY — Don't Repeat Yourself

**Definition and intent:** Keep each authoritative piece of knowledge, rule, or representation in one place so it cannot drift. Repeated text is evidence to inspect, not automatic proof of a violation.

**Signals:** The same business rule, formula, schema, mapping, or validation changes in several places; copied implementations have already diverged; generated artifacts are maintained manually beside their source.

**Ask:** Must these occurrences always change together? Do they represent the same concept for the same reason? Is there a stable name and contract for the shared knowledge?

**Useful moves:** Extract a function or value; centralize a business policy; derive generated data from one source; create a module after the common concept is understood.

**Do not overapply:** Similar code in unrelated domains may be coincidental duplication. Premature sharing couples independent change paths and creates flag-heavy abstractions. Prefer duplication over the wrong abstraction until the shared concept is stable.

**Test code inverts the default:** Inside test bodies, readable narrative beats deduplication, because the cost of indirection is paid on every failure investigation. Read [Readability over reuse](test-automation.md#readability-over-reuse-damp-over-dry) before extracting anything from a test. Locators, waits, auth plumbing, and request construction remain shared knowledge; the arrange/act/assert narrative and expected values do not.

**Interactions:** DRY can conflict with SRP when a shared utility accumulates unrelated logic, and with KISS/YAGNI when abstraction is speculative.

## KISS — Keep It Simple

**Definition and intent:** Choose the least complex design that clearly satisfies known requirements and constraints. Simplicity includes the whole lifecycle, not merely the fewest source lines.

**Signals:** Indirection has no current consumer; a familiar algorithm is replaced by clever chaining or metaprogramming; configuration exceeds behaviour; readers must traverse many layers for one rule.

**Ask:** What complexity is essential? Which option is easiest to explain, test, operate, and change? Does removing a layer lose a real boundary? Is the concise version actually clearer to this team?

**Useful moves:** Direct control flow; explicit data structures; remove pass-through layers; narrow configuration; use standard language features and existing conventions.

**Do not overapply:** “Simple” does not mean simplistic. Security, concurrency, transactional correctness, observability, and performance requirements can justify complexity. Encapsulation may add a type while reducing system complexity.

**Interactions:** KISS constrains every other principle and is especially important when OCP, DIP, or DRY suggest new abstractions.

## YAGNI — You Aren't Gonna Need It

**Definition and intent:** Do not implement speculative capabilities before a present requirement or strong, costly-to-reverse constraint justifies them.

**Signals:** Unused extension points, configuration for hypothetical providers, generic frameworks with one client, dormant fields, or planned features embedded in today's domain model.

**Ask:** Which accepted requirement needs this now? What is the cost of adding it later? Is there a cheap seam that preserves reversibility without implementing the feature? Does a legal, security, data, or compatibility constraint require early action?

**Useful moves:** Delete unused capability; implement the current path directly; record future ideas outside production code; make irreversible choices explicit; add only a low-cost seam when delay would be expensive.

**Do not overapply:** YAGNI does not excuse unmaintainable code or ignore foreseeable migrations, public API compatibility, data retention, security, and regulatory constraints. Deliberate extensibility can be justified by evidence.

**Interactions:** YAGNI limits speculative OCP and DIP structures; KISS guides the current implementation.

## Composition over Inheritance

**Definition and intent:** Assemble independently varying behaviours through collaborators instead of inheriting implementation from a rigid hierarchy. Composition improves replaceability and local reasoning.

**Signals:** Deep hierarchies, combinatorial subclasses, protected-state coupling, fragile-base-class regressions, or capabilities that do not form a true “is-a” relationship.

**Ask:** Is the subtype semantically valid and substitutable? Do behaviours vary independently or need runtime selection? Is the base contract stable? Does composition clarify ownership or only add forwarding methods?

**Useful moves:** Inject behaviour objects or functions; use decorators for orthogonal concerns; compose capability interfaces; delegate instead of exposing inherited internals.

**Use inheritance when:** The relationship is genuine, stable, shallow, behaviourally substitutable, and the base abstraction intentionally supports extension. Framework base types and closed taxonomies can be reasonable.

**Interactions:** LSP determines whether inheritance is safe. Composition commonly supports SRP, ISP, OCP, and DIP, but unnecessary delegation can violate KISS.

## Law of Demeter — Principle of Least Knowledge

**Definition and intent:** A unit should collaborate through the direct contracts it needs rather than navigating the internal object graph of other units. This limits structural coupling and protects encapsulation.

**Signals:** `a.getB().getC().doThing()` crosses entity or service boundaries; callers know collection and storage layout; changes to an internal relationship ripple through distant clients.

**Ask:** Is the chain navigating internal structure or using one cohesive fluent abstraction? Which object owns the requested behaviour? Would delegation improve the domain boundary or merely hide harmless data access?

**Useful moves:** Tell an object what outcome is needed; add a meaningful domain operation; delegate through a service; pass the direct collaborator; return a purpose-built view rather than mutable internals.

**Do not overapply:** Fluent builders, query APIs, immutable value transformations, and chains operating on values returned by the same abstraction are not automatically violations. Wrapper methods that only mirror every nested operation can increase indirection.

**Interactions:** The Law of Demeter reduces coupling alongside DIP and encapsulation. Too many forwarding methods conflict with KISS.

## Common pitfalls and responses

| Pitfall | Evidence to seek | Proportional response |
|---|---|---|
| Over-engineering | Speculative variants, unused layers, excessive configuration | Apply KISS/YAGNI; remove or postpone unneeded machinery |
| Tight coupling | Provider details in policy, broad ripple changes, hard infrastructure tests | Introduce the narrowest boundary justified by volatility |
| Ignoring reuse | The same knowledge changes repeatedly in several places | Centralize the stable rule, not merely similar syntax |
| Insufficient abstraction | Implementation details leak and stable concepts lack names | Extract a contract around the demonstrated concept |
| Premature abstraction | Generic framework precedes a second real use; flags multiply | Restore direct implementations until commonality is known |
| Over-fragmentation | Many pass-through types obscure one cohesive workflow | Recombine behaviour that changes together |
| Principle theatre | Patterns or interfaces exist without a risk they mitigate | Remove ceremony; document the actual constraint |

## Case study: legacy CMS

**Starting problem:** One CMS component validates users, persists profiles, edits content, and publishes it. User policy, storage, content rules, and delivery change independently; tests require unrelated setup.

**Smallest justified redesign:** Keep one application-level use-case façade if it is convenient, but delegate to cohesive collaborators such as `UserRepository`, `ContentPolicy`, and `Publisher`. Do not split every operation into its own class. The gain is localized change and focused tests, not a higher type count.

**Relevant principles:** SRP identifies independent reasons to change; ISP keeps consumer-facing contracts narrow; DIP is justified only at external or volatile boundaries; KISS prevents over-fragmentation.

## Case study: notification system

**Starting problem:** `NotificationService` constructs email and SMS clients and branches on channel type. Provider replacement and isolated tests are now required, but callers still select exactly one of the two existing channels.

**Smallest justified redesign:** Define one consumer-shaped `Notifier` capability with `send(message)`, inject providers, and preserve selection of one channel per call. Keep provider translation inside adapters. Existing construction may retain default providers for compatibility; move all wiring to a composition root only when changing callers is in scope. If only email exists and no second channel is planned, a direct implementation may still be simpler.

**Relevant principles:** DIP guides the consumer contract; composition makes providers replaceable. A fixed channel set can retain a direct branch; YAGNI prevents a plugin framework beyond current needs.
