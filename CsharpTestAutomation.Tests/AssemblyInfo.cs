// Parallel Execution
// Fixtures are allowed to run in parallel, but the level is 1. Application tests share one
// bootstrapped session and one QA database. Do not raise the level until test data is unique
// per test.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
