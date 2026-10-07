# Behavioural evaluation

This is a maintainer resource. Do not load it for ordinary design tasks.

Load `cases.json`, create each case's files in an isolated workspace, and send only its prompt and raw files to the model. For explicit-use tests, supply the skill. For discovery tests, make the skill available in the host catalog without forcing invocation. Keep this rubric out of the tested model's context. Use the same host instructions and fixtures for baseline and candidate runs. Do not use live services.

## Pass criteria

| Case | Required result |
| --- | --- |
| notification-refactor | Allows fake providers; keeps zero-argument construction and `notify(channel, message)`; sends once through the selected channel; preserves provider errors; no unrelated edits. |
| independent-duplication | Accepts independent policies and avoids extraction based only on equal syntax. |
| costly-defect | Reports that the returned map exposes mutable internal state despite the snapshot contract; describes runtime mutation risk and remedy without suppressing the issue because repair is costly. |
| four-alternatives | Compares all four requested options, includes costs, and avoids speculative plugin machinery. |
| short-explanation | Gives two accurate sentences, with no compulsory follow-up questions. |
| mixed-language | Identifies `displayName` versus `name`, the ineffective type assertion, and the resulting runtime failure; examines both sides. |
| negative-trigger | Gives `tsc --version` or its equivalent without selecting the design skill. |
| valid-fluent-api | Does not flag the fluent chain itself as a design violation. |
| damp-over-dry | Declines further extraction; names the boolean flags selecting per-caller behaviour and the assertions hidden from the specs; recommends returning the narrative and expectations to the tests while keeping locators shared. Does not treat the two similar test bodies as a DRY violation. |
| base-test-god-class | Places both concerns in composed per-concern extensions or fixtures rather than in the base class or a new subclass layer; flags the `static` `Page` as parallel-unsafe. |
| parallel-unsafe-page-object | Identifies `static current` and `static lastUser` as shared mutable state that breaks parallel runs; recommends a per-test instance supplied by a fixture. Does not settle for reducing worker count. |
| flaky-retry-request | Refuses retries as a fix absent a root cause; replaces `waitForTimeout` with a retrying web-first assertion on observable state; raises that the product may have a real race that retries would conceal. |
| playwright-nunit-parallelism | Distinguishes NUnit's general lifecycle options from the Playwright NUnit runner's supported `ParallelScope.Self`; rejects `ParallelScope.All` with `PageTest` despite `InstancePerTestCase`; recommends supported fixture parallelism and states that no browser checks ran. |
| mixed-artifact-design | Assesses the service and test responsibilities together; proposes optional provider injection with compatible defaults; keeps expectations visible in the spec; removes the need to replace global `console.log`; does not force one production/test reference choice for the whole task. |
| transport-contract | Accepts status, header, and JSON assertions because they are the explicit contract under test; keeps request/auth knowledge in the client; does not recommend hiding required response metadata behind a domain-only result. |
| mechanical-rename | Gives one sentence about renaming the declaration and its uses; preserves behavior; does not select the design skill, suggest abstractions, or edit files. |

Grade semantic outcomes and observable actions; do not require exact headings or wording. Use relevant executable checks for generated changes. Record missed defects, false findings, scope expansion, unnecessary questions/tool calls, and task completion. Preserve failures and unrun cases explicitly.

## Scenarios without fixtures

These expectations have no entry in `cases.json`. Exercise them by hand when changing the skill, using the language guides in `references/` as the input. Evaluate decisions and evidence, not exact wording.

| Scenario | Required result |
| --- | --- |
| C# registration service mixes validation, storage, and messaging | Identifies the independent reasons to change; retains cohesive orchestration; abstracts only boundaries with demonstrated need; preserves behaviour. |
| Java `Bird.fly()` hierarchy includes a penguin | Flags LSP; models `Flying` as a separate capability; proposes contract tests rather than an override that throws. |
| First implementation has one variant and only hypothetical future variants | Balances OCP against YAGNI and normally keeps the direct design. |
| Stable, shallow, substitutable taxonomy | Allows inheritance and explains why composition is not automatically superior. |
| Review request carrying only naming or formatting preferences | Produces no design finding unless a material consequence is demonstrated. |

## Run record

Record the date, skill content hashes or revision, model ID, host/version, effort/thinking settings, case ID, invocation mode, transcript/artifacts, check results, and grader rationale. Compare the previous and revised skill on the same host/model/settings, then compare with a no-skill run when measuring added value. Test the models the team actually uses; select available model IDs in the runner rather than prescribing them here. Repeat variable cases and compare observed work and outcomes rather than assuming equal effort labels imply equal compute.

For reference-routing changes, inspect the read trace: did the agent load the required sections and their exceptions, miss a linked constraint, or load unrelated language guides? Keep the grader and expected outcomes out of the task model's context. The Waza adapter is a smoke subset; its keyword checks do not prove these semantic outcomes. Grade the saved response and actions against the row above before declaring a behavioral pass. A host/session error is unavailable evidence, not a design failure.

No model runs are recorded by shipping these fixtures. An unrun case is not a pass. Use an available authorised evaluation environment; do not invent model results.
