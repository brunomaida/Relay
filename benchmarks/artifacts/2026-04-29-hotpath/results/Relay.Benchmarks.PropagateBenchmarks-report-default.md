
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method                          | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
-------------------------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
 Depth1_Healthy_Default          | 0.7205 ns | 1.5128 ns | 0.0829 ns |  1.00 |    0.00 |     222 B |         - |          NA |
 Depth1_Healthy_Propagate_NoNext | 0.7566 ns | 1.3043 ns | 0.0715 ns |  1.05 |    0.08 |     229 B |         - |          NA |
 Depth2_Propagate_Fork           | 3.3235 ns | 2.2325 ns | 0.1224 ns |  4.66 |    0.60 |     229 B |         - |          NA |
 Depth2_Fork_Wrapped             | 5.6308 ns | 0.5694 ns | 0.0312 ns |  7.88 |    0.88 |     214 B |         - |          NA |
