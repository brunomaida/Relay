```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                  | ItemCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|------------------------ |---------- |----------:|----------:|----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| **Push_Single**             | **100000**    |  **6.088 ms** |  **3.454 ms** | **0.1894 ms** |  **1.00** |    **0.00** | **19.5313** | **19.5313** | **19.5313** |  **64.94 KB** |        **1.00** |
| Push_Single_SlowBackend | 100000    |  4.960 ms |  7.406 ms | 0.4060 ms |  0.81 |    0.05 | 19.5313 | 19.5313 | 19.5313 |  64.94 KB |        1.00 |
|                         |           |           |           |           |       |         |         |         |         |           |             |
| **Push_Single**             | **1000000**   | **21.080 ms** | **17.841 ms** | **0.9779 ms** |  **1.00** |    **0.00** |       **-** |       **-** |       **-** |  **64.93 KB** |        **1.00** |
| Push_Single_SlowBackend | 1000000   | 12.701 ms | 24.180 ms | 1.3254 ms |  0.60 |    0.06 | 15.6250 | 15.6250 | 15.6250 |  64.94 KB |        1.00 |
