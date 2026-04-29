```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                       | Capacity | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|----------------------------- |--------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| **Spsc_TryPublish_Baseline**     | **64**       | **3.6227 ns** | **2.5324 ns** | **0.1388 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 64       | 7.8947 ns | 0.3370 ns | 0.0185 ns |  2.18 |    0.08 |     203 B |         - |          NA |
| Mpsc_TryPublish_Full         | 64       | 0.3605 ns | 2.0057 ns | 0.1099 ns |  0.10 |    0.03 |     140 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 64       | 0.3759 ns | 0.8836 ns | 0.0484 ns |  0.10 |    0.02 |      61 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **1024**     | **3.7944 ns** | **1.3757 ns** | **0.0754 ns** |  **1.00** |    **0.00** |     **178 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 1024     | 7.9676 ns | 2.6005 ns | 0.1425 ns |  2.10 |    0.06 |     203 B |         - |          NA |
| Mpsc_TryPublish_Full         | 1024     | 0.4939 ns | 0.1224 ns | 0.0067 ns |  0.13 |    0.00 |     140 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 1024     | 0.3772 ns | 1.1526 ns | 0.0632 ns |  0.10 |    0.02 |      61 B |         - |          NA |
|                              |          |           |           |           |       |         |           |           |             |
| **Spsc_TryPublish_Baseline**     | **65536**    | **4.1363 ns** | **1.7391 ns** | **0.0953 ns** |  **1.00** |    **0.00** |     **184 B** |         **-** |          **NA** |
| Mpsc_TryPublish_NoContention | 65536    | 8.6952 ns | 1.3118 ns | 0.0719 ns |  2.10 |    0.04 |     203 B |         - |          NA |
| Mpsc_TryPublish_Full         | 65536    | 0.5907 ns | 0.2104 ns | 0.0115 ns |  0.14 |    0.01 |     140 B |         - |          NA |
| Mpsc_TryConsume_Empty        | 65536    | 0.3393 ns | 2.1709 ns | 0.1190 ns |  0.08 |    0.03 |      61 B |         - |          NA |
