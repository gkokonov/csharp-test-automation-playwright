# Source Coverage and Validation

This file supports maintainers and auditors. It is not required for ordinary skill use.

## Source baseline

Primary baseline: Arslan Ahmad, [Essential Software Design Principles (SOLID) – Must-Knows Before Your Interview](https://www.designgurus.io/blog/essential-software-design-principles-you-should-know-before-the-interview), DesignGurus, dated 13 April 2026 and accessed 18 August 2026.

The harness paraphrases the source. It preserves the article's intended topics—definitions, benefits, application guidance, pitfalls, and case studies—but does not reproduce the article or its examples verbatim.

Additional references used to check or enrich important claims:

- Barbara Liskov and Jeannette Wing, [A Behavioral Notion of Subtyping](https://www.cs.columbia.edu/~wing/publications/LiskovWing94.pdf), for behavioural rather than syntactic substitutability.
- The Northeastern University Demeter project, [Demeter: A CASE Study of Software Growth](https://www2.ccs.neu.edu/research/demeter/papers/growth-parameterized/journal-oop.pdf), for the coupling context behind the Law of Demeter.
- The official TypeScript handbook on [interfaces and structural typing](https://www.typescriptlang.org/docs/handbook/interfaces.html), [type compatibility](https://www.typescriptlang.org/docs/handbook/type-compatibility.html), and [discriminated unions](https://www.typescriptlang.org/docs/handbook/unions-and-intersections.html).
- OpenAI's [Build skills](https://learn.chatgpt.com/docs/build-skills) documentation for `SKILL.md`, progressive disclosure, `agents/openai.yaml` metadata, and `.agents/skills` discovery. `https://developers.openai.com/codex/skills` serves the same page; append `.md` to either URL for the source Markdown.
- The [Agent Skills specification](https://agentskills.io/specification), which the OpenAI documentation names as the standard skills build on, for the authoritative frontmatter field limits and the progressive-disclosure budgets.
- GitHub's [Copilot customization cheat sheet](https://docs.github.com/en/copilot/reference/customization-cheat-sheet) and [Agent Skills documentation](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills) for shared `.agents/skills` packaging.
- Devin's [AGENTS.md guidance](https://docs.devin.ai/desktop/cascade/agents-md) for the provider-neutral repository adapter used by Devin and Windsurf/Cascade environments.

## Article traceability matrix

| Source section | Harness location | Treatment |
| --- | --- | --- |
| Purpose and benefits of design principles | `SKILL.md`; `principles-and-tradeoffs.md` introduction | Retained as maintainability, clarity, testability, and proportional evolution goals |
| SOLID overview | `principles-and-tradeoffs.md` → SOLID | Retained and made diagnostic rather than checklist-driven |
| Single Responsibility Principle | `principles-and-tradeoffs.md` → SRP; C# case | Reframed around reason/actor for change; warns against one-method classes |
| Open-Closed Principle | `principles-and-tradeoffs.md` → OCP | Corrects “never modify” interpretation; requires demonstrated variation |
| Liskov Substitution Principle | `principles-and-tradeoffs.md` → LSP; `java.md` | Replaces the flightless subtype that throws with separate capabilities and contract checks |
| Interface Segregation Principle | `principles-and-tradeoffs.md` → ISP; language guides | Replaces invalid JavaScript multiple-class-inheritance syntax with capability interfaces/functions |
| Dependency Inversion Principle | `principles-and-tradeoffs.md` → DIP; C#/TypeScript cases | Distinguishes dependency inversion from dependency-injection mechanics and interface-per-class ceremony |
| DRY | `principles-and-tradeoffs.md` → DRY | Distinguishes duplicated knowledge from coincidental syntactic similarity |
| KISS | `principles-and-tradeoffs.md` → KISS | Defines simplicity across understanding, testing, operation, and change—not line count |
| YAGNI | `principles-and-tradeoffs.md` → YAGNI | Retains current-needs focus while protecting costly-to-reverse security, data, and compatibility decisions |
| Composition over inheritance | `principles-and-tradeoffs.md`; `typescript.md` | Repairs non-callable ability objects; preserves inheritance for genuine stable substitutable taxonomies |
| Law of Demeter | `principles-and-tradeoffs.md`; `typescript.md` fluent example | Distinguishes object-graph navigation from cohesive fluent builders and value APIs |
| Over-engineering | Common pitfalls table; workflow conflict rules | Maps to KISS/YAGNI and requires proportional solutions |
| Tight coupling | Common pitfalls table; DIP and Demeter sections | Adds evidence and smallest-boundary guidance |
| Ignoring reusability | Common pitfalls table; DRY section | Corrects the assumption that all reuse is beneficial |
| Insufficient abstraction | Common pitfalls table | Balances missing abstraction against premature abstraction |
| Legacy CMS case study | `principles-and-tradeoffs.md` → legacy CMS; `csharp.md` | Keeps cohesive use-case orchestration while separating independent policy/I/O changes |
| Notification case study | `principles-and-tradeoffs.md` → notification system; `typescript.md` | Uses one consumer-shaped capability and composition without speculative plugin machinery |
| Conclusion and FAQs | `workflows-and-output-contracts.md` | Converted from interview summary into reusable modes and output contracts |

## Deliberate corrections and enrichments

1. **LSP:** Throwing from an inherited `fly` operation remains a contract violation; a `FlightlessBird` layer does not repair it. The capability must not be promised by non-flying birds.
2. **ISP in JavaScript:** JavaScript and TypeScript do not allow `class X extends A, B, C`. TypeScript interfaces or composed objects model the capabilities correctly.
3. **Composition example:** Storing `new Walks()` and calling the object as `ability()` fails. A composed capability must expose and invoke a callable method or be stored as a function.
4. **DRY:** Square and rectangle formulas that look similar do not alone prove duplicated knowledge. Sharing is justified only when the concepts must evolve together.
5. **KISS:** A functional pipeline is not inherently more complex than a loop; clarity depends on team conventions, semantics, allocation/performance needs, and error handling.
6. **OCP and YAGNI:** Extension is valuable at real change axes; speculative frameworks increase complexity and maintenance cost.
7. **Law of Demeter:** Chain length is an unreliable proxy. Structural navigation across collaborators is the concern; fluent operations within one abstraction can remain encapsulated.
8. **Abstraction and reuse:** Both too little and too much abstraction are failure modes. The harness requires a stable concept and concrete benefit.

## Additions beyond the source baseline

[Test Automation Design](test-automation.md) and the three `*-testing.md` language guides are **not** derived from the source article. They were added because this skill's primary use is test automation and test framework development, which the article does not address. Trace them to tool documentation and established test-smell literature rather than to the traceability matrix above.

[csharp-testing.md](csharp-testing.md) assumes **NUnit** and separates .NET-specific guidance from NUnit mechanics, with a mapping table for xUnit and MSTest. Its NUnit lifecycle, parallel-scope, assertion, and `TestContext` claims were checked against the NUnit documentation on `FixtureLifeCycle`, `Parallelizable`, `OneTimeSetUp`, `TestContext`, and the NUnit 4 breaking-changes and migration notes. Re-verify them against the NUnit version in use before relying on version-sensitive details such as classic-assert namespaces or `[CancelAfter]`.

The most consequential addition is the DAMP-over-DRY inversion for test code. Left unstated, the general [DRY](principles-and-tradeoffs.md#dry--dont-repeat-yourself) guidance pushes extraction that damages failure diagnosis in a suite, so the DRY section now carries an explicit pointer. When editing either location, keep the two consistent: general DRY governs the system under test, and the test-automation file governs test code.

Each language guide is split into a production guide and a `*-testing.md` guide so a task loads only the relevant half, and so test-automation guidance can grow without inflating the production guides. Cross-language material stays in `test-automation.md` and is deliberately not restated in the per-language guides.

Known gap: module and package-level dependency principles (acyclic dependencies, stable dependencies) are deliberately out of scope. Only the one-way test-layer ordering in [Layering and dependency direction](test-automation.md#layering-and-dependency-direction) is defined. `SKILL.md` and the verification scale still mention dependency direction in a general sense without a full treatment.

## Maintaining this package

Behavioural expectations live in exactly one place: the [grading rubric](../evals/rubric.md). It holds the pass criteria for the fixtured prompts in [cases.json](../evals/cases.json) plus the scenarios that have no fixture. Do not restate them here; a second copy would drift from the fixtures it describes. Those criteria state intended behaviour and are not proof of model execution. Keep model settings and run records in the evaluation environment.

One checker covers every language. Run [check_examples.py](../scripts/check_examples.py) with Python 3 and the toolchains for the languages you are checking:

```sh
python scripts/check_examples.py                 # C#, Java, and TypeScript
python scripts/check_examples.py --only java     # one language
```

It scans `references/csharp*.md`, `references/java*.md`, and `references/typescript*.md`, so examples added to a new guide are covered automatically. Each fenced block carries a marker above it:

```text
<!-- example: <id> | verify: <tokens> -->
```

Tokens are comma-separated: `compile`, `run`, `contract:<name>`, or `none` with an optional parenthesised reason (commas inside the reason are preserved). For TypeScript, `compile` means `tsc --strict --target ES2020`. The Markdown declares what to check; the script owns the check logic, the expected stdout, and the contract probes. Blocks can therefore be reordered, added, or removed without silently losing coverage—the run fails on an unmarked block, an unknown token, an unknown contract, a duplicate id, or a missing `REQUIRED_IDS` entry. `REQUIRED_IDS` is keyed by language, so `--only` still enforces the subset it covers. It lists every example including the `verify: none` test-automation blocks, which are never compiled and for which this presence check is therefore the only guard against deletion or renaming.

C# blocks build with `TreatWarningsAsErrors` against `--framework` (default `net10.0`); Java blocks compile with `-Xlint:all -Werror`. The notification contracts additionally check the before/after public API, single-channel delivery, injected providers, message identity, and propagation of errors without fallback delivery. The checker uses temporary files and downloads nothing. Use `--node`, `--tsc-js`, `--dotnet`, `--javac`, and `--java-bin` for tools outside PATH; when invoking Windows tools from WSL, set `--work-dir` to a workspace on a Windows-mounted drive. Missing tools or failed checks produce a nonzero exit status.

When adding an example, add a marker. Use `verify: none (<reason>)` for a block that needs packages this package does not install, such as the Playwright and NUnit/JUnit examples in the `*-testing.md` guides; that records the decision instead of leaving the block silently unchecked.

### Authoring basis — refreshed 15 September 2026

The review used current [OpenAI skill guidance](https://developers.openai.com/codex/skills), [GPT-6 Astra prompting guidance](https://developers.openai.com/api/docs/guides/latest-model), [Anthropic skill guidance](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices), [Opus 5 guidance](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5), and [Sonnet 5 guidance](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-sonnet-5). These support selective loading, flexible task scope, proportionate checks, and model-specific evaluation. They do not establish performance for this skill without actual runs.

## Current validation record — 15 September 2026

Scope: this standalone skill after the test-automation additions, the production/test guide split, the unified marker-driven checker, and twelve evaluation fixtures. A later same-day revision consolidated the workflow into `SKILL.md`, replaced its prose routing with a load table, removed restated paragraphs from the three `*-testing.md` guides, extended `REQUIRED_IDS` to the test-automation blocks, and fixed two defects in `typescript-testing.md`. Only the rows marked re-run below were re-executed for that revision; the compile rows predate it and cover files it did not touch.

| Check | Result |
| --- | --- |
| Frontmatter limits | Passed, re-checked against the Agent Skills specification. `name` 26 chars, lowercase-hyphen, no leading/trailing/consecutive hyphen, matches the parent directory as the spec requires. `description` 476 chars, within the spec's 1,024-character maximum and Copilot's 1,024-character limit. A previous record cited a "Codex 500-character limit"; no such per-description limit exists. The spec's 500-character cap applies to the unused `compatibility` field, and the Codex 2%-of-context/8,000-character budget applies to the initial skills *list*, not to one description. |
| Description behaviour under a crowded catalog | Noted, not a pass or failure. Codex shortens skill descriptions first when many skills are installed, so trigger words should be front-loaded. The `Excludes …` clause is last in the description and is therefore the first content lost, which is exactly what the `negative-trigger` fixture exercises. Untested: no crowded-catalog run has been performed. |
| `agents/openai.yaml` schema and location | Passed. The path matches the documented skill layout (sibling of `SKILL.md`, `scripts/`, `references/`). All four keys used—`interface.display_name`, `interface.short_description`, `interface.default_prompt`, `policy.allow_implicit_invocation`—are documented; the `$skill` mention syntax in `default_prompt` is correct. `allow_implicit_invocation` defaults to `true`, so setting it to `true` restates the default and is kept only as declared intent. Checked by key comparison against the documented example, not by a host load or `skills-ref validate`. |
| Local file links and heading anchors | Passed, re-run after the routing-table and mode-section revision. All 59 relative targets and `#anchor` fragments resolve. |
| Unfinished-marker scan | Passed, re-run. |
| C# production examples | Passed under `check_examples.py`. Both `csharp.md` blocks build on .NET SDK 10.0.111 targeting `net10.0` with `TreatWarningsAsErrors` and both print `Welcome user@example.com`, evidencing the refactor preserved behaviour. |
| Java production examples | Passed under `check_examples.py`. Both `java.md` blocks compile with `javac -Xlint:all -Werror` on JDK 25.0.4 and run on the matching JRE. Outputs differ by design: `BirdsBefore` prints `Eagle flies` while `BirdsAfter` prints `Penguin lays an egg` then `Eagle flies`, because that pair demonstrates capability separation rather than behaviour preservation. |
| TypeScript production examples | Passed under `check_examples.py`. The three marked blocks type-check with TypeScript 5.1.3 (`--strict --target ES2020`) and run on Node v22.23.2. Not latest-compiler validation. An earlier record claimed Node 26.8.2; v22.23.2 is what this environment actually ran. |
| Notification contract checks | Passed for selected delivery, compatible construction/method calls, injected providers, message identity, and error propagation without fallback. |
| Checker guard behaviour | Passed. Induced faults each produced a nonzero exit, in every language: unmarked C#/Java/TypeScript block, duplicate id across languages, renamed required id (including under `--only`), unknown verify token, unknown contract name, drifted Java stdout, a C# warning under `TreatWarningsAsErrors`, broadcast delivery, swallowed provider error, and drifted TypeScript stdout. The id-presence guard was re-checked in-process after `REQUIRED_IDS` was extended—renaming `playwright-after`, `junit-before`, or `nunit-after` is reported as missing, deleting the `nunit-before` marker trips the unmarked-block guard instead, and `--only java` correctly ignores a renamed C# id. That subset was exercised by importing the module, not by a full CLI run. |
| Test-automation examples | **Not compiled.** The C#, Java, and TypeScript test-automation blocks require NUnit/xUnit, Playwright, RestSharp, JUnit 5, and REST Assured. The TypeScript ones in `typescript-testing.md` are marked `verify: none`, so the checker reports them as skipped rather than passing. They are reviewed prose-and-code, not executed evidence. All six are now listed in `REQUIRED_IDS`, so they are guarded against deletion and renaming even though they are never compiled; a missing import in `playwright-before` had gone undetected until it was found by review, which is the residual risk this guard does not remove. |
| Evaluation fixtures | Twelve cases parse with local fixture paths, including four new test-automation cases; grading criteria are stored separately. |
| Model behaviour and discovery | Not run on Opus 5, Sonnet 5, or other separate model sessions. Fixtures are ready; no model-level pass is claimed. |

Code execution covers every production example in all three languages, the notification contracts, and the checker's own guard behaviour. It does not establish full runtime correctness beyond those entrypoints, does not cover any `*-testing.md` example, and says nothing about model effectiveness.

When a check is not run, preserve that fact. Do not replace it with an inferred pass.
