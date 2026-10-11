---
applyTo: "**"
trigger: always
description: "Project-specific correction history. Read before every task; append whenever a correction is made."
---

# Project Lessons

Read [`docs/LESSONS.md`](../../docs/LESSONS.md) at the start of every session,
before reading any other project guidance.

Apply every rule listed there without exception. These rules reflect explicit
corrections; treat them as higher priority than general best practices.

## When to append a new lesson

Append a lesson entry to `docs/LESSONS.md` whenever the user corrects an agent
decision, rejects generated code, or changes a pattern the agent chose. One
correction → one entry. Never repeat the same mistake twice.

### Entry format

```markdown
## YYYY-MM-DD — <Short title of the rule>

**Rule:** <Precise, actionable statement of what to do or not do.>

**Why:** <What went wrong and what the user said or showed that revealed the problem.>

**Applies when:** <Conditions or contexts where this rule must be followed.>
```

### Writing rules

- Write rules as direct imperatives: "Do X" / "Never do Y".
- Scope to this repository; avoid generic advice already covered by base guidance.
- Keep each entry self-contained. A new session must be able to apply it without
  reading the surrounding conversation.
- Do not rephrase or summarize; copy the user's exact words in the "Why" field
  when they are precise enough.
- Prefer a narrow scope over a vague one. A rule that applies everywhere and
  means nothing is worse than no rule.
