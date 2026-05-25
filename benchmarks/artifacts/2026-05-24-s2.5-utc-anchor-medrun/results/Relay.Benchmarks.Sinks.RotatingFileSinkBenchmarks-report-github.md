```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]    : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  MediumRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=MediumRun  IterationCount=15  LaunchCount=2  
WarmupCount=10  

```
| Method                 | Mean     | Error    | StdDev   | Allocated |
|----------------------- |---------:|---------:|---------:|----------:|
| ShouldRotate_Predicate | 21.20 ns | 0.091 ns | 0.127 ns |         - |
| ShouldRotate_HotPath   | 68.40 ns | 6.679 ns | 9.997 ns |         - |
