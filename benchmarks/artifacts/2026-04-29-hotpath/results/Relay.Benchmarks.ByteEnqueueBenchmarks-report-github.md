```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                    | Mean     | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|-------------------------- |---------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Byte_Healthy       | 1.035 ns | 0.4096 ns | 0.0225 ns |  1.00 |    0.00 |     407 B |         - |          NA |
| Depth2_Byte_AcceptReject  | 5.810 ns | 0.4298 ns | 0.0236 ns |  5.61 |    0.12 |     406 B |         - |          NA |
| Depth2_Byte_HeadUnhealthy | 4.521 ns | 0.4242 ns | 0.0233 ns |  4.37 |    0.11 |     407 B |         - |          NA |
| Depth3_Byte_AllUnhealthy  | 8.664 ns | 0.5161 ns | 0.0283 ns |  8.37 |    0.16 |     415 B |         - |          NA |
