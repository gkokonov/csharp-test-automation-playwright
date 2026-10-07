# Workflows and Output Contracts

Select the primary mode from the current request and conversation. Combine modes when authorised, such as “review and fix,” and continue into implementation when the user later requests the fix. Adapt output sections to the requested length and format.

The workflow in `SKILL.md` runs in every mode and is not repeated here. Each section below adds only what that mode changes, plus its output contract.

## Mode index

- [Design](#design-mode)
- [Implementation/refactoring](#implementationrefactoring-mode)
- [Review](#review-mode)
- [Explain/interview](#explaininterview-mode)
- [Conflict resolution](#conflict-resolution-rules)
- [Verification scale](#verification-scale)

## Design mode

### Adds

1. State quality attributes and integration boundaries alongside the behaviour and constraints.
2. By default, compare the current/direct option with at most two justified alternatives. Compare more only when the user requests them or the decision requires them.
3. Recommend the simplest option that meets present requirements *and* preserves costly-to-reverse constraints.
4. Define public interfaces or data flow only as far as implementation requires; identify deferred capabilities explicitly.

### Output

- **Recommendation:** one clear choice.
- **Forces and evidence:** current needs and change axes.
- **Alternatives:** benefits, costs, and why they were not selected.
- **Design:** responsibilities, collaborators, contracts, and data flow.
- **Risks and verification:** failure modes, tests, compatibility, and deferred decisions.

## Implementation/refactoring mode

### Adds

1. Establish the non-goals as well as the authorised change; do not ask for confirmation already supplied. Read callers as well as the affected code, contracts, and tests.
2. Capture current behaviour with existing tests, or a focused characterization test when feasible.
3. The contract to preserve includes persistence formats, exceptions, ordering, and side effects, not only public API signatures.
4. Implement using repository conventions. Avoid opportunistic cleanup outside the affected boundary.
5. Follow the correction loop in `SKILL.md`. If a required check cannot run, report the blocker and its effect on confidence; use the permitted checks that remain available. An unavailable check is not a pass.

### Output

- Lead with what changed and why it is safer or easier to evolve.
- Name only the principles that materially drove the edit.
- List affected contracts and compatibility decisions.
- Report tests/checks and any remaining verification gap.
- If no refactor is justified, explain why and leave the code unchanged.

## Review mode

### Finding threshold

Investigate candidate issues before filtering the final report. Honour the requested review scope and any explicit severity filter. Report findings with:

1. Concrete code evidence and a tight location.
2. A plausible change, defect, test, or maintenance scenario.
3. Material impact beyond personal style.
4. The smallest useful remedy, or a clear statement that the remedy needs further investigation. Repair cost does not invalidate an evidenced defect; report cost and uncertainty separately.

Do not report the absence of a pattern, interface, abstraction, or dependency injection by itself.

### Finding format

```text
[severity] Short outcome-focused title
Evidence: file/symbol and observed design.
Impact: concrete failure or maintenance consequence.
Principle: the heuristic that explains the risk.
Recommendation: smallest useful correction.
Trade-off: added cost or reason the current design may be acceptable.
Confidence: high, medium, or low with the key assumption.
```

Order findings by impact. Merge symptoms with the same root cause. If there are no material findings, say that directly and mention any validation limitation.

### Example: evidence supports a finding

Input: `balances.ts:8` returns the internal `Map` as a `ReadonlyMap` while promising a snapshot.

Output: “The snapshot exposes live internal state. A JavaScript caller can mutate the returned map, and later deposits also change an earlier snapshot. Return a copy behind the required read-only contract. This adds a copy cost; no runtime checks were run.”

### Example: no design change is justified

Input: Two teams independently require identifiers of at least eight characters; both validators use `s.length >= 8`.

Output: “Keep the validators separate. Their policies have independent owners and can change for different reasons. Equal syntax does not establish shared knowledge; extracting one validator would couple those policies.”

## Explain/interview mode

### Procedure and output

1. Give a one-sentence definition in original wording.
2. Explain the problem it addresses and the observable signal.
3. Give one idiomatic example or short scenario.
4. Give a counterexample or over-application warning.
5. State a trade-off and how it interacts with another principle.
6. For interview practice, add follow-up questions when useful. For short explanations, use only the elements that fit the requested length.

For interview answers, prefer this narrative: **problem → principle → design choice → trade-off → verification**. Do not present slogans as absolute laws.

## Conflict-resolution rules

Use the relevant **Interactions** and **Do not overapply** sections in [Principles and Trade-offs](principles-and-tradeoffs.md#principle-index). They are the maintained source for conflicts between principles.

- **Local requirements versus heuristics:** follow explicit repository requirements and the requested scope. Evidence of a material problem can justify proposing a change to the owning convention; explain the migration cost and change that convention only when authorized.

## Verification scale

- **Explanation only:** check conceptual accuracy and example semantics.
- **Local refactor:** focused unit/contract tests plus build/type-check of affected module.
- **Public contract change:** caller search, compatibility tests, migration notes, and broader suite.
- **Architecture change:** dependency direction checks, integration tests, operational concerns, and staged rollout where relevant.
- **Test or test-framework change:** run the affected specs and check the order and concurrency risks of the changed scope. Use parallel execution supported by the runner; one green run does not establish isolation. For a change to the surface specs consume—fixtures, base classes, page objects, builders, configuration keys—count callers across the suite, run a representative cross-section, and supply the migration.

Never convert an unrun check into a success claim. State “not run” and the reason.
