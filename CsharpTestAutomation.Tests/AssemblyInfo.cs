// Parallel Execution
// Fixtures run in parallel to exercise the framework's per-test isolation (per-instance RestSharp
// clients, AsyncLocal timing in ApiLoggingInterceptor). Level is kept moderate because these tests
// call an external service; raise or lower it per environment/rate-limit constraints.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
