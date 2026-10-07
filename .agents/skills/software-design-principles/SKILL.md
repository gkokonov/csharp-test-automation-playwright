---
name: software-design-principles
description: >-
  Guides software design, refactoring, architecture reviews, and explanations
  when responsibilities, coupling, contracts, or change cost need analysis.
  Covers C#, Java, and TypeScript/JavaScript, including test framework design,
  page objects, fixtures, test data, parallel safety, and design causes of flaky
  tests. Applies SOLID, DRY/DAMP, KISS, YAGNI, composition, and the Law of Demeter
  to concrete evidence. Excludes mechanical renames, formatting, dependency
  updates, and failures with a clear product or configuration cause.
---

# Software Design Principles

Use software design principles as context-sensitive heuristics. Optimize for demonstrated needs, behavioural correctness, compatibility, clarity, testability, and proportional change scope—not textbook conformance or pattern count.

Follow the user's scope and the target repository's instructions, contracts, and toolchain. The references give design heuristics and illustrative examples; they do not replace local rules or authorize extra actions. When a version-sensitive tool decision matters, check the installed version and its official documentation rather than assuming an example applies unchanged.

## Select the mode

Infer the mode and authorised scope from the request and conversation. Ask only when unresolved information would materially change the work. Follow the user's requested scope, length, and format; the output templates are defaults.

| Request signal | Mode | Scope decision |
|---|---|---|
| “How should we build…?”, “Which design…?” | **Design** | Compare minimal designs for new or changing behaviour. |
| “Refactor…”, “Improve this design”, or a readability change involving responsibilities, coupling, contracts, or change cost | **Implementation/refactoring** | Change code while preserving authorised contracts and behaviour. A purely mechanical rename or formatting cleanup is not a design refactor. |
| “Review…”, “What is wrong with this design?”, or “Find design risks” | **Review** | Identify evidence-backed design risks without modifying code unless asked. |
| “Explain…”, “Teach…”, or interview practice | **Explain/interview** | Teach or rehearse a principle with examples and trade-offs. |
| “Review and fix…”, “Refactor and explain…” | **Combined modes** | Run the requested modes in this order unless the user specifies otherwise: design, review, implementation/refactoring, explain/interview. Preserve each selected mode's output contract and verify any implementation. |

When a request matches multiple modes, apply the combined-mode order above and ask only if that order could change the result. If the repository or referenced files cannot be inspected, state that limitation, restrict conclusions to the supplied material, and do not claim repository-specific evidence.

## What to load

Use each reference's index to select relevant sections. Include its introduction, applicable constraints, exceptions, and interactions before judging the design. Read a complete example when needed; loading every reference is unnecessary.

| Read | When |
|---|---|
| [workflows-and-output-contracts.md](references/workflows-and-output-contracts.md) | A substantial task, for the current mode. A brief explanation may need no reference. |
| [principles-and-tradeoffs.md](references/principles-and-tradeoffs.md) | A principle's diagnostic detail helps decide or explain. |
| [test-automation.md](references/test-automation.md) | The artifact under design is a test, a suite, or a test automation framework. |
| A language guide below | A language-specific decision matters; read its design choices and review checks. |
| [source-coverage-and-validation.md](references/source-coverage-and-validation.md) | Auditing this harness, tracing it to the source article, or maintaining its examples. |
| [Evaluation rubric](evals/rubric.md), [cases](evals/cases.json), and [example checker](scripts/check_examples.py) | Maintaining or evaluating the skill; keep grading criteria out of the task model's context. Ordinary design tasks do not need them. |

| Language | Production code | Test code |
|---|---|---|
| C# or .NET | [csharp.md](references/csharp.md) | [csharp-testing.md](references/csharp-testing.md) — NUnit, xUnit, MSTest, Playwright, RestSharp |
| Java | [java.md](references/java.md) | [java-testing.md](references/java-testing.md) — JUnit 5, Playwright, REST Assured |
| TypeScript or JavaScript | [typescript.md](references/typescript.md) | [typescript-testing.md](references/typescript-testing.md) — Playwright Test |

- Select guides for the artifacts and responsibilities involved. A task spanning a service and its tests can need both columns; a cross-language contract can need multiple language guides. Load only their relevant sections.
- Read a complete example only when it helps the current problem; prefer TypeScript over plain JavaScript.
- For a tool-level test decision read `test-automation.md` with the matching `*-testing.md`: the first carries the cross-language principles, the second only its tool specifics.
- For test code, `test-automation.md` overrides the general DRY pressure described in the DRY section of [principles-and-tradeoffs.md](references/principles-and-tradeoffs.md): a test's dominant quality is diagnostic speed under failure.

## Workflow

Every mode runs these steps; the mode sections add only their differences and output contract.

1. Establish the requested outcome, affected behaviour, language/runtime, compatibility constraints, and allowed change scope.
2. If a repository is available, inspect its nearest instructions, public contracts, tests, representative implementations, and established patterns before judging the design.
3. Identify observable forces: responsibilities that change independently, variation points already requested, dependency direction, behavioural contracts, duplicated knowledge, navigation coupling, and accidental complexity.
4. Name a principle only when it explains a concrete risk or decision. Separate an actual violation from a possible future improvement.
5. Compare the current design with the smallest viable alternative. Include costs such as extra types, indirection, migration risk, runtime overhead, or learning burden.
6. Prefer no change when the expected benefit is speculative, the duplication is coincidental, or the abstraction would exceed current requirements.
7. For changes, preserve behaviour and public APIs unless their alteration is explicitly in scope. Reuse existing tests and seams. Add a seam only when needed to verify affected behaviour and worth its design cost. Run the required checks, correct defects introduced by the change, then repeat affected checks. Broaden verification only for new evidence or unresolved risk. For test frameworks, preserve the surface specs consume and verify relevant order and concurrency risks using the runner's supported execution modes.
8. Lead with the outcome. Give the evidence, recommendation, trade-off, and verification that the task needs. Keep responses and written deliverables proportional to the request.

## Non-negotiable quality rules

- Do not apply all principles to every task or introduce a design pattern merely to name one.
- Do not equate SRP with “one method per class,” OCP with “never edit code,” DIP with “interface for every class,” DRY with removing every similar line, or the Law of Demeter with banning fluent chains.
- Judge LSP by observable contracts: accepted inputs, outputs, state transitions, side effects, errors, and invariants. A subtype that throws for a promised capability is not substitutable.
- Prefer composition for independently varying capabilities, but retain inheritance when the subtype relationship is genuine, stable, shallow, and behaviourally substitutable.
- Distinguish missing abstraction from premature abstraction. Require evidence of a stable concept or change axis.
- In review mode, investigate candidates before applying the reporting threshold. Report evidence-backed defects even when their repair is costly. Omit style-only or speculative findings and honour an explicit user severity filter.
- In implementation mode, do not broaden the refactor to adjacent code without authorization.
- Never claim compilation, tests, or runtime validation that was not performed.
- Never present a retry, a longer timeout, or a re-run as a fix for a flaky test without naming the root cause. Concealing a product defect behind a passing suite is a correctness failure, not a trade-off.
