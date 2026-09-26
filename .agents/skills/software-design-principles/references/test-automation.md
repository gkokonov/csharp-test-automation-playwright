# Test Automation Design

Test code is production code with a different cost function. Its dominant quality is **diagnostic speed under failure**: how fast an engineer who did not write the test can tell whether the product broke, the environment broke, or the test lied. Reuse, type count, and abstraction elegance are secondary to that.

Read this file when the artifact under design is a test, a test suite, or a test automation framework. Where its guidance differs from [Principles and Trade-offs](principles-and-tradeoffs.md#principle-index) for test code, this file takes precedence; the general file still governs the system under test.

## Index

- [Readability over reuse](#readability-over-reuse-damp-over-dry)
- [Layering and dependency direction](#layering-and-dependency-direction)
- [Page, screen, and component objects](#page-screen-and-component-objects)
- [Fixtures, hooks, and base classes](#fixtures-hooks-and-base-classes)
- [Test data](#test-data)
- [Determinism and synchronization](#determinism-and-synchronization)
- [Isolation and parallel safety](#isolation-and-parallel-safety)
- [Boundaries and hermeticity](#boundaries-and-hermeticity)
- [Assertions](#assertions)
- [The framework's public API](#the-frameworks-public-api)
- [Test smells](#test-smells)
- [Review checks](#review-checks)

## Readability over reuse (DAMP over DRY)

**Definition and intent:** Prefer Descriptive And Meaningful Phrases in test bodies. A test should read as a self-contained narrative—arrange, act, assert—without the reader opening other files. This deliberately inverts the default DRY pressure that applies to production code.

**Why the inversion:** A production duplication bug costs a divergent rule. A test-indirection bug costs every future failure investigation. Helpers that save keystrokes routinely cost minutes per failure, multiplied across the suite and across engineers.

**Still keep in one place—this knowledge must not drift:** locators and selectors, waiting/synchronization logic, authentication and session plumbing, API request construction, domain rules the suite asserts against, environment and configuration resolution.

**Leave duplicated—this is narrative, not knowledge:** the arrange/act/assert sequence, literal expected values, per-test input data, and the specific assertions a test makes.

**Signals of over-extraction:** a test body that is nothing but helper calls; assertions hidden inside shared helpers so the test shows no expectation; a helper taking boolean or enum flags so each caller selects different behaviour; needing three or more files to learn what one test does; a `TestHelpers` or `CommonSteps` module that grows with every new test.

**Ask:** If this test fails at 2am, can the reader see the expected value without navigating away? Is the thing I am extracting *knowledge* that must change together, or *narrative* that happens to look similar?

**Do not overapply:** DAMP is not a licence for copy-pasted locators, duplicated waits, or forked API clients. Repeating a literal `"user@example.com"` is fine; repeating a CSS selector in nine specs is a defect.

**Interactions:** This bounds DRY for test code. It does not weaken SRP, DIP, or the Law of Demeter, which still apply to the framework layers underneath the tests.

## Layering and dependency direction

**Definition and intent:** Test automation has a natural one-way dependency order. Keep it one-way.

```text
test / spec  →  task or flow objects (optional)  →  page / screen / component objects  →  driver or transport wrappers  →  system under test
```

**The rule:** Each layer may depend on the layer below it and never on the layer above. Concretely: a page object must not import a spec, a fixture, or the suite's assertion vocabulary; a driver wrapper must not contain business vocabulary; two page objects should not import each other cyclically.

**Heuristic:** If you deleted the entire test layer, everything below it should still compile. If it does not, a lower layer depends on a test.

**Signals:** Page objects that assert; page objects that read test data files; a driver wrapper naming domain concepts like `Checkout`; a base page importing a concrete page; utilities that import specs for constants; cyclic imports between screen objects.

**Useful moves:** Move shared constants down, never up. Give the lower layer a parameter instead of letting it reach upward. Put cross-page journeys in a flow/task object above the page layer rather than inside one page.

**Do not overapply:** The flow/task layer is optional. Do not add it until multi-page journeys are actually repeated. A small suite may legitimately have only specs, page objects, and a driver wrapper.

## Page, screen, and component objects

**Definition and intent:** A page or component object owns the knowledge of how to locate and operate one meaningful piece of UI, and exposes that as intentions in the user's language.

**Signals of a problem:** Methods named after widgets (`GetUsernameInput`) rather than intentions (`LogIn`); element handles or driver types returned to the test; locators appearing in specs; a single object covering an entire application; assertions embedded so tests cannot state their own expectations.

**Ask:** Does this object expose *what the user does* or *what the DOM contains*? Does the test need to know any selector? Does the object map to a real UI composition boundary?

**Useful moves:** Expose intention methods that return either a value or the next page/component object. Encapsulate every locator. Model reusable widgets (grids, dialogs, nav bars) as component objects composed into pages, mirroring the real UI composition. Expose state as queries (`IsErrorVisible()`, `VisibleRowCount()`) so tests own the assertion.

**Do not overapply:** Not every DOM fragment deserves a class. Do not create a page object per URL when several URLs are one screen, and do not wrap a capable framework API in a homegrown pass-through layer that adds no vocabulary.

**Interactions:** Locator encapsulation is the Law of Demeter applied to UI: tests tell the page what outcome they want instead of navigating its element graph.

## Fixtures, hooks, and base classes

**Definition and intent:** Shared setup should be composed in, not inherited. Prefer the framework's fixture or dependency-injection mechanism over a base class hierarchy.

**Signals:** A `BaseTest` that owns driver lifecycle, configuration, logging, reporting, test data, and API clients at once; subclasses overriding setup and depending on a specific `super` call order; a test that cannot be understood without reading two or three ancestors; adding one field to the base class affecting every test in the suite; `protected` state shared implicitly between base and subclass.

**Ask:** Which tests need this setup—all of them, or the ones I happened to put under this parent? Is this an "is-a" relationship or just a place to put shared code? What is the fixture's scope, and is that scope visible at the point of use?

**Useful moves:** Express each concern as its own fixture and request only the fixtures a test needs. Make scope explicit and narrow (per test by default; per class or per suite only for genuinely expensive, immutable setup). Use composition for shared behaviour that is not a subtype relationship.

**Do not overapply:** A shallow, stable base class that only wires the framework's own lifecycle is acceptable, and some frameworks require one. The defect is accumulation and depth, not the existence of a base class.

**Interactions:** This is composition over inheritance and SRP applied to the suite's setup path. A god base class is the canonical test framework design smell.

## Test data

**Definition and intent:** A test should state exactly the data it depends on and nothing more.

**Signals:** Tests reading records created by other tests or by a seeding script (Mystery Guest); shared mutable fixture objects; hardcoded IDs from a shared environment; tests that pass alone and fail in a suite because data was consumed; setup that builds a large object when the test cares about one field.

**Useful moves:** Test data builders or factories with valid defaults plus explicit overrides of only the fields under test—the overrides document the test's intent. Generate unique identifiers per test whenever data touches shared state. Create what you need in the test (or its fixture) and clean up by ownership.

**Do not overapply:** Immutable reference data (country lists, product catalogues) can be shared safely. Builders for trivial value types are ceremony.

## Determinism and synchronization

**Definition and intent:** Flakiness is a design defect with a root cause, not an environmental fact of life. Treat an intermittent test as a bug in the test, the framework, or the product—and find out which.

**Signals:** Fixed sleeps; retry wrappers that make a suite green without a diagnosis; assertions racing asynchronous updates; tests that depend on execution order; dependence on wall-clock time, locale, or timezone; waits scattered through specs instead of owned by one layer.

**Useful moves:** Wait on observable state with explicit conditions, and own that logic in the page or driver layer. Inject clock, randomness, and identifier sources where determinism matters. When a test is flaky, classify the cause—product race, missing synchronization, shared state, or environment—before changing anything.

**Do not overapply:** Retries are a reporting and triage tool. They may buy time on a known, tracked product race; they never substitute for a diagnosis, and retrying to conceal a product defect is a correctness failure, not a trade-off.

## Isolation and parallel safety

**Definition and intent:** Every test must be able to run alone, in any order, and concurrently with others.

**Signals:** Static or singleton driver, page, or client instances; a shared logged-in session that tests mutate; global configuration mutated during a run; a shared pool of test accounts; framework-level mutable statics such as a current-page tracker; `static` collections used as caches.

**Ask:** What state is shared, and who mutates it? Is each page object held for the lifetime of one test only? If two tests run at the same instant, which of them corrupts the other?

**Useful moves:** Scope mutable state to a test and pass it explicitly. Make framework singletons immutable or thread-local by deliberate decision rather than by accident. Give each parallel worker its own account, tenant, or data namespace.

**Do not overapply:** Expensive immutable setup—a parsed configuration, a started browser process, a read-only reference dataset—can legitimately be shared at suite scope.

## Boundaries and hermeticity

**Definition and intent:** DIP applied to the system under test. Wrap each external system behind a narrow, consumer-shaped client so tests express requests and expectations in domain terms.

**Signals:** HTTP verbs, headers, status codes, and JSON paths appearing directly in specs; a client that mirrors a provider SDK one-to-one instead of what tests need; UI tests silently depending on a third-party sandbox; database cleanup logic inlined in tests.

**Useful moves:** Give the API client domain operations (`CreateOrder(order)`) that return typed results, keeping serialization, auth, and retry policy inside. Shape the client around test needs rather than the transport library. State explicitly which dependencies are real and which are stubbed, and why.

**Do not overapply:** A single API call used by one test does not need a client class. Do not stub the thing you are trying to test—an integration test that stubs the integration proves nothing.

## Assertions

**Definition and intent:** Each test verifies one logical outcome, and its failure message should identify the problem without a rerun.

**Signals:** Many unlabelled assertions in sequence so a failure does not say which concern broke (Assertion Roulette); one test exercising several unrelated behaviours (Eager Test); assertions on implementation detail such as internal DOM structure or call counts that the requirement does not mention; conditional logic (`if`/`try`) inside a test so it can pass down multiple paths.

**Useful moves:** One logical outcome per test—several physical assertions are fine when they describe that one outcome. Attach context or descriptive messages, or use soft assertions that report every failure with its label. Assert on observable behaviour. Make the failure message carry expected, actual, and the identifying context.

**Do not overapply:** Splitting one outcome into several tests that repeat the same expensive setup trades diagnosis clarity for runtime and duplication.

## The framework's public API

**Definition and intent:** A test automation framework's consumers are test authors. Its public API is the set of fixtures, base classes, page and component objects, builders, helpers, configuration keys, and reporting hooks that specs depend on.

**Implications:** Treat that surface as a published contract. Breaking it is a migration across every spec in the suite, usually owned by people who are not the framework author. Renaming a fixture is not a refactor; it is a breaking change with a caller count.

**Useful moves:** Before changing the surface, search the suite for callers and report the count. Prefer additive change. When a break is justified, provide the mechanical migration and stage it. Keep internal helper layers genuinely internal so they stay free to change.

**Do not overapply:** Internal plumbing that no spec imports is not a public contract and should not be frozen as if it were.

## Test smells

| Smell | Evidence to seek | Proportional response |
| --- | --- | --- |
| God base class | Base test owning driver, config, data, logging, and reporting; deep override chains | Split into per-concern fixtures; request only what a test needs |
| Over-abstracted test | Test body is only helper calls; assertions live in helpers | Inline the narrative back into the test; keep only knowledge shared |
| Mystery guest | Test depends on data it did not create | Create the data in the test or its fixture; make it unique |
| Sleepy test | Fixed sleeps; timing-tuned waits | Wait on observable state; own the condition in the page/driver layer |
| Fragile test | One UI change breaks many specs; selectors in specs | Encapsulate locators; use stable test-oriented attributes |
| Assertion roulette | Many unlabelled assertions; failure does not say what broke | Label assertions or use soft assertions with context |
| Erratic test | Passes alone, fails in suite or in parallel | Find the shared mutable state; scope it to the test |
| Leaky transport | Headers, status codes, JSON paths in specs | Move transport into a consumer-shaped client |
| Retry as a fix | Green suite with retry counts and no diagnosis | Classify the root cause; keep retries only as tracked triage |
| Conditional test logic | `if`/`try` branches in tests | Split into separate tests with explicit expectations |

## Review checks

- Does the failing test tell the reader what was expected without opening another file?
- Is anything shared that is *narrative* rather than *knowledge*, or duplicated that is *knowledge* rather than *narrative*?
- Do any dependencies point upward—page objects reaching into specs, fixtures, or data files?
- Do specs contain locators, sleeps, headers, status codes, or JSON paths?
- Could this suite run in a random order and in parallel? What shared mutable state says otherwise?
- Is shared setup composed as fixtures with visible scope, or inherited from an accumulating base class?
- Does each test create the data it depends on, and is that data unique where it touches shared state?
- Do page objects expose intentions and state queries rather than widgets and element handles?
- Are retries or waits concealing a product defect that should be reported instead?
- Does a proposed framework change break the surface specs import, and how many callers were counted?
