```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                              | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|------------------------------------ |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Byte_Healthy                 | 0.1714 ns | 0.0300 ns | 0.0280 ns |  1.00 |    0.00 |     407 B |         - |          NA |
| Depth2_Byte_AcceptReject            | 6.1833 ns | 0.1494 ns | 0.1398 ns | 36.94 |    5.89 |     408 B |         - |          NA |
| Depth2_Byte_HeadUnhealthy           | 2.5777 ns | 0.0492 ns | 0.0436 ns | 15.27 |    2.46 |     406 B |         - |          NA |
| Depth3_Byte_AllUnhealthy            | 3.3055 ns | 0.0433 ns | 0.0405 ns | 19.75 |    3.13 |     437 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Healthy      | 0.2430 ns | 0.0153 ns | 0.0135 ns |  1.44 |    0.24 |     139 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Reject       | 0.2317 ns | 0.0349 ns | 0.0326 ns |  1.39 |    0.34 |     126 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Unhealthy | 3.7092 ns | 0.0368 ns | 0.0344 ns | 22.15 |    3.41 |     348 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Reject    | 4.0866 ns | 0.0803 ns | 0.0751 ns | 24.42 |    3.88 |     348 B |         - |          NA |
