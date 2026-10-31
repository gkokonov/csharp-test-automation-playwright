# Implementation workflow

Use this workflow only when the user explicitly selects or requests the custom
Implementator role. Ordinary implementation requests stay with the main agent
and use root/scoped guidance and applicable skills. Keep one writer per task;
role selection authorizes no commit, publication, or cloud execution.

## Start and scope

1. Read root AGENTS.md, tooling's guidance reading checklist, and each affected
   project entry point. Follow its linked canonical rules, layer architecture,
   and the feature specification required by the task.
2. Load the installed user-level `software-design-principles` skill for every
   implementation. Resolve it through the host's skill catalog; its user-level
   location is `~/.agents/skills/software-design-principles/SKILL.md`.
   Apply it in proportion to the change; read only
   relevant references. Do not introduce abstractions without concrete need.
3. Select Framework, NUnit application, independent BDD application, or framework
   self-tests by responsibility. Ask BDD versus NUnit if it changes implementation.
   Cross-project tasks use all affected scopes; do not create four writers.
4. Record the shared [task contract](../instructions/tooling.md#task-contract),
   including baseline, authorized scope, constraints, and intended checks.
5. Give a short plan for substantial work. Resolve dependent questions before
   making those changes; continue independent authorized work where possible.

## Implement and validate

Use existing patterns and canonical policy. Update the owning guidance when a
convention changes. Keep architecture and feature documents as reference owners.
Use [tooling's role access rules](../instructions/tooling.md#agent-tool-access)
for navigation, documentation, browser diagnostics, and external tool caches.
Prepare [complete review evidence](../instructions/tooling.md#rtk-and-complete-evidence)
when RTK condenses routine command output.
Run the checks required by the affected scope and the user's request. Distinguish
build, analyzer, discovery, local self-test, and live-service results. Report
blocked prerequisites; a review does not replace execution evidence.

## Review and research

Follow the shared [review workflow](../review/REVIEW.md) for independent final
review, findings, fixes, and revalidation. Use the shared
[bounded research route](../instructions/tooling.md#bounded-research) for lookups
and authorized delegation.

## Finish

Report changed files, checks actually performed, review coverage and dispositions,
remaining issues, and host limitations. No publishing, committing, or cloud
execution is authorized by selecting this role.
