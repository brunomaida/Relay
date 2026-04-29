```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]    : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  MediumRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=MediumRun  IterationCount=15  LaunchCount=2  
WarmupCount=10  

```
| Method                 | Mean     | Error    | StdDev   | Allocated |
|----------------------- |---------:|---------:|---------:|----------:|
| ShouldRotate_Predicate | 21.84 ns | 0.151 ns | 0.222 ns |         - |
| ShouldRotate_HotPath   | 69.23 ns | 5.115 ns | 7.498 ns |         - |
