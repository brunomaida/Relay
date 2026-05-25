```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                              | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|------------------------------------ |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Byte_Healthy                 | 0.2076 ns | 0.0074 ns | 0.0069 ns |  1.00 |    0.00 |     407 B |         - |          NA |
| Depth2_Byte_AcceptReject            | 3.7428 ns | 0.0192 ns | 0.0180 ns | 18.05 |    0.62 |     404 B |         - |          NA |
| Depth2_Byte_HeadUnhealthy           | 1.2436 ns | 0.0094 ns | 0.0088 ns |  6.00 |    0.21 |     422 B |         - |          NA |
| Depth3_Byte_AllUnhealthy            | 3.2736 ns | 0.0691 ns | 0.0646 ns | 15.79 |    0.61 |     437 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Healthy      | 0.2205 ns | 0.0061 ns | 0.0058 ns |  1.06 |    0.04 |     139 B |         - |          NA |
| Depth1_Byte_TryEnqueue_Reject       | 0.2187 ns | 0.0036 ns | 0.0032 ns |  1.05 |    0.03 |     126 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Unhealthy | 3.6690 ns | 0.0169 ns | 0.0158 ns | 17.69 |    0.62 |     348 B |         - |          NA |
| Depth1_Byte_Drop_NextNull_Reject    | 3.8599 ns | 0.0123 ns | 0.0102 ns | 18.52 |    0.61 |     348 B |         - |          NA |
