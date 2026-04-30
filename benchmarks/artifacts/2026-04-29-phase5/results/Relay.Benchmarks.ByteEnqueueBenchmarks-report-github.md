```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                              | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|------------------------------------ |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Byte_Healthy                 | 2.6820 ns | 0.1059 ns | 0.0058 ns |  1.00 |    0.00 |     407 B |         - |          NA |
| Depth2_Byte_AcceptReject            | 1.2611 ns | 0.1363 ns | 0.0075 ns |  0.47 |    0.00 |     370 B |         - |          NA |
| Depth2_Byte_HeadUnhealthy           | 2.1589 ns | 0.1045 ns | 0.0057 ns |  0.80 |    0.00 |     434 B |         - |          NA |
| Depth3_Byte_AllUnhealthy            | 3.0662 ns | 0.8954 ns | 0.0491 ns |  1.14 |    0.02 |     437 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Healthy      | 0.4193 ns | 0.0168 ns | 0.0009 ns |  0.16 |    0.00 |     139 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Reject       | 0.2240 ns | 0.0689 ns | 0.0038 ns |  0.08 |    0.00 |     126 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Unhealthy | 3.7507 ns | 0.4156 ns | 0.0228 ns |  1.40 |    0.01 |     348 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Reject    | 3.7868 ns | 0.3353 ns | 0.0184 ns |  1.41 |    0.01 |     348 B |         - |          NA |
