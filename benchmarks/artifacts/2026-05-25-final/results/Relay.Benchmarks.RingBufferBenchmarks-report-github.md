```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method            | Capacity | Mean       | Error     | StdDev    | Median     | Ratio  | RatioSD | Code Size | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|-----------:|-------:|--------:|----------:|----------:|------------:|
| **TryConsume_Empty**  | **64**       |  **0.2020 ns** | **0.0097 ns** | **0.0075 ns** |  **0.2022 ns** |   **1.00** |    **0.00** |      **73 B** |         **-** |          **NA** |
| RoundTrip         | 64       |  0.7354 ns | 0.0461 ns | 0.0453 ns |  0.7220 ns |   3.61 |    0.24 |     178 B |         - |          NA |
| TryPublish_Full   | 64       |  0.0265 ns | 0.0179 ns | 0.0167 ns |  0.0273 ns |   0.15 |    0.08 |     110 B |         - |          NA |
| RoundTrip_Batch32 | 64       | 37.9179 ns | 0.6574 ns | 0.8548 ns | 37.8221 ns | 188.44 |    8.08 |     422 B |         - |          NA |
|                   |          |            |           |           |            |        |         |           |           |             |
| **TryConsume_Empty**  | **1024**     |  **0.1853 ns** | **0.0129 ns** | **0.0120 ns** |  **0.1837 ns** |   **1.00** |    **0.00** |      **73 B** |         **-** |          **NA** |
| RoundTrip         | 1024     |  1.0469 ns | 0.0523 ns | 0.0623 ns |  1.0481 ns |   5.63 |    0.55 |     178 B |         - |          NA |
| TryPublish_Full   | 1024     |  0.0481 ns | 0.0323 ns | 0.0346 ns |  0.0301 ns |   0.27 |    0.20 |     110 B |         - |          NA |
| RoundTrip_Batch32 | 1024     | 55.1181 ns | 0.8317 ns | 0.7780 ns | 54.9431 ns | 298.72 |   21.30 |     431 B |         - |          NA |
|                   |          |            |           |           |            |        |         |           |           |             |
| **TryConsume_Empty**  | **65536**    |  **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |  **0.0000 ns** |      **?** |       **?** |      **73 B** |         **-** |           **?** |
| RoundTrip         | 65536    |  1.8284 ns | 0.0729 ns | 0.2126 ns |  1.8502 ns |      ? |       ? |     184 B |         - |           ? |
| TryPublish_Full   | 65536    |  0.1955 ns | 0.0178 ns | 0.0158 ns |  0.1916 ns |      ? |       ? |     110 B |         - |           ? |
| RoundTrip_Batch32 | 65536    | 65.8362 ns | 1.3528 ns | 1.7591 ns | 65.7963 ns |      ? |       ? |     431 B |         - |           ? |
