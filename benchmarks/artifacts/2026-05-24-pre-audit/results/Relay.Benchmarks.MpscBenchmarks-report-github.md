```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                       | Capacity | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|----------------------------- |--------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| **Spsc_TryPublish_Baseline**     | **64**       | **1.9475 ns** | **0.0129 ns** | **0.0108 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 64       | 6.4100 ns | 0.0721 ns | 0.0675 ns |  3.29 |    0.04 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 64       | 0.2151 ns | 0.0074 ns | 0.0069 ns |  0.11 |    0.00 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 64       | 0.2176 ns | 0.0062 ns | 0.0055 ns |  0.11 |    0.00 |     145 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **1024**     | **1.1639 ns** | **0.0142 ns** | **0.0133 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 1024     | 6.7696 ns | 0.0586 ns | 0.0519 ns |  5.81 |    0.08 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 1024     | 0.2460 ns | 0.0093 ns | 0.0082 ns |  0.21 |    0.01 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 1024     | 0.2183 ns | 0.0056 ns | 0.0046 ns |  0.19 |    0.00 |     145 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **65536**    | **1.7373 ns** | **0.0677 ns** | **0.1623 ns** |  **1.00** |    **0.00** |     **184 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 65536    | 7.5309 ns | 0.0712 ns | 0.0631 ns |  4.19 |    0.39 |     291 B |         - |          NA |
| Mpsc_TryPublish_Full         | 65536    | 0.2085 ns | 0.0054 ns | 0.0048 ns |  0.12 |    0.01 |     164 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 65536    | 0.2213 ns | 0.0097 ns | 0.0091 ns |  0.12 |    0.01 |     145 B |         - |          NA |
