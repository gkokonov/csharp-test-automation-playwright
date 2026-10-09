// Reqnroll features run in parallel; scenarios in each feature run sequentially.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]
