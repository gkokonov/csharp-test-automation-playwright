# Commit Message Instructions

## Format

```text
<type>(automation): <short description> #<work-item-id>
```

## Allowed Types

* `feat` - new functionality
* `fix` - bug fix
* `refactor` - code restructuring without behavior changes
* `test` - test additions or updates
* `docs` - documentation changes
* `chore` - maintenance or configuration changes

## Rules

* Follow Conventional Commits.
* Keep the first line to 72 characters or fewer.
* Use lowercase and imperative mood: `add`, `fix`, `update`, `remove`.
* Use the `automation` scope.
* Include the Azure DevOps work item ID.
* Do not end the first line with a period.
* Describe the actual change. Avoid generic messages.

## Good Examples

```text
feat(automation): add default currency selection #8711
feat(automation): send billing emails after bill approval #94423
fix(automation): correct SOFR interest calculation #31822
```

## Multi-line Example

```text
refactor(automation): simplify disbursement booking #54442

- improve class and method names
- fix code formatting
- split large classes by responsibility
```

## Bad Examples

```text
commit1
Update
Automation work
Saurabh code commit
```
