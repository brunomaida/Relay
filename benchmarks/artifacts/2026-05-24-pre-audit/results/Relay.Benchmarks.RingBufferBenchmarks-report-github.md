```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method            | Capacity | Mean       | Error     | StdDev    | Median     | Ratio   | RatioSD | Code Size | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|-----------:|--------:|--------:|----------:|----------:|------------:|
| **TryConsume_Empty**  | **64**       |  **0.2159 ns** | **0.0074 ns** | **0.0069 ns** |  **0.2150 ns** |   **1.000** |    **0.00** |      **73 B** |         **-** |          **NA** |
| RoundTrip         | 64       |  0.6510 ns | 0.0067 ns | 0.0059 ns |  0.6504 ns |   3.007 |    0.09 |     178 B |         - |          NA |
| TryPublish_Full   | 64       |  0.0019 ns | 0.0041 ns | 0.0036 ns |  0.0000 ns |   0.009 |    0.02 |     110 B |         - |          NA |
| RoundTrip_Batch32 | 64       | 36.1127 ns | 0.2455 ns | 0.2050 ns | 36.0036 ns | 166.694 |    4.94 |     422 B |         - |          NA |
|                   |          |            |           |           |            |         |         |           |           |             |
| **TryConsume_Empty**  | **1024**     |  **0.2063 ns** | **0.0033 ns** | **0.0029 ns** |  **0.2064 ns** |    **1.00** |    **0.00** |      **73 B** |         **-** |          **NA** |
| RoundTrip         | 1024     |  0.9448 ns | 0.0112 ns | 0.0104 ns |  0.9404 ns |    4.58 |    0.09 |     178 B |         - |          NA |
| TryPublish_Full   | 1024     |  0.0025 ns | 0.0049 ns | 0.0045 ns |  0.0000 ns |    0.01 |    0.02 |     110 B |         - |          NA |
| RoundTrip_Batch32 | 1024     | 48.5355 ns | 1.0030 ns | 2.1804 ns | 47.3255 ns |  247.93 |    6.61 |     431 B |         - |          NA |
|                   |          |            |           |           |            |         |         |           |           |             |
| **TryConsume_Empty**  | **65536**    |  **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |  **0.0000 ns** |       **?** |       **?** |      **73 B** |         **-** |           **?** |
| RoundTrip         | 65536    |  2.2848 ns | 0.1106 ns | 0.3138 ns |  2.1769 ns |       ? |       ? |     184 B |         - |           ? |
| TryPublish_Full   | 65536    |  0.2046 ns | 0.0049 ns | 0.0041 ns |  0.2032 ns |       ? |       ? |     110 B |         - |           ? |
| RoundTrip_Batch32 | 65536    | 62.0749 ns | 0.2489 ns | 0.2207 ns | 62.0580 ns |       ? |       ? |     431 B |         - |           ? |
