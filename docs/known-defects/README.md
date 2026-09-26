# Known Defect Reports

Store one report per approved known defect at `docs/known-defects/<id>.md`. Link the report to its regression test with `[Category("KnownDefect")]` and `[AllureIssue("<id>")]`.

Use this template:

```markdown
# <ID>: <Short title>

Status: Open | Accepted | Fixed
Owner: <team or role>
Last reviewed: YYYY-MM-DD

## Observed behavior
<What happens and when>

## Expected behavior
<What should happen>

## Impact
<Users, data, or test coverage affected>

## Workaround
<Current workaround, or None>

## Regression test
<Fully qualified test name and test project>
```
