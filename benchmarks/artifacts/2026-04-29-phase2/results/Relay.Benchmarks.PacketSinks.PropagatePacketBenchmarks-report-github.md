```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                          | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|-------------------------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Healthy_Default          |  1.273 ns | 1.2113 ns | 0.0664 ns |  1.00 |    0.00 |     407 B |         - |          NA |
| Depth1_Healthy_Propagate_NoNext |  1.429 ns | 0.4980 ns | 0.0273 ns |  1.12 |    0.08 |     403 B |         - |          NA |
| Depth2_Propagate_Fork           | 10.081 ns | 0.6285 ns | 0.0344 ns |  7.93 |    0.45 |     418 B |         - |          NA |
| Depth2_Fork_Wrapped             |  8.403 ns | 5.7280 ns | 0.3140 ns |  6.60 |    0.19 |     400 B |         - |          NA |
