```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                       | Capacity | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|----------------------------- |--------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| **Spsc_TryPublish_Baseline**     | **64**       | **0.9297 ns** | **0.0132 ns** | **0.0124 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 64       | 7.7089 ns | 0.0650 ns | 0.0608 ns |  8.29 |    0.14 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 64       | 0.2074 ns | 0.0052 ns | 0.0046 ns |  0.22 |    0.01 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 64       | 0.2100 ns | 0.0068 ns | 0.0063 ns |  0.23 |    0.01 |     145 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **1024**     | **1.3344 ns** | **0.0077 ns** | **0.0072 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 1024     | 6.6202 ns | 0.0305 ns | 0.0271 ns |  4.96 |    0.02 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 1024     | 0.2075 ns | 0.0045 ns | 0.0040 ns |  0.16 |    0.00 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 1024     | 0.2115 ns | 0.0070 ns | 0.0062 ns |  0.16 |    0.00 |     145 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **65536**    | **2.0967 ns** | **0.0818 ns** | **0.2334 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 65536    | 7.5000 ns | 0.1763 ns | 0.2529 ns |  3.55 |    0.30 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 65536    | 0.2047 ns | 0.0221 ns | 0.0206 ns |  0.10 |    0.01 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 65536    | 0.2359 ns | 0.0362 ns | 0.0339 ns |  0.11 |    0.02 |     145 B |         - |          NA |
