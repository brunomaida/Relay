```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method               | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Healthy       | 0.4781 ns | 0.7876 ns | 0.0432 ns |  1.00 |    0.00 |     148 B |         - |          NA |
| Depth2_AcceptReject  | 2.9866 ns | 0.3755 ns | 0.0206 ns |  6.28 |    0.58 |     152 B |         - |          NA |
| Depth2_HeadUnhealthy | 2.5889 ns | 1.1995 ns | 0.0657 ns |  5.45 |    0.60 |     150 B |         - |          NA |
| Depth3_AllUnhealthy  | 3.1620 ns | 2.4158 ns | 0.1324 ns |  6.65 |    0.70 |     150 B |         - |          NA |
