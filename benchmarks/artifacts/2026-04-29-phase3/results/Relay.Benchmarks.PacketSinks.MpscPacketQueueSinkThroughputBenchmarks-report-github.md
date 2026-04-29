```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                  | ItemCount | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|------------------------ |---------- |----------:|-----------:|----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| **Push_Single**             | **100000**    |  **7.317 ms** |  **5.3299 ms** | **0.2922 ms** |  **1.00** |    **0.00** | **15.6250** | **15.6250** | **15.6250** |  **64.84 KB** |        **1.00** |
| Push_Single_SlowBackend | 100000    |  2.476 ms |  0.5161 ms | 0.0283 ms |  0.34 |    0.01 | 19.5313 | 19.5313 | 19.5313 |  64.84 KB |        1.00 |
|                         |           |           |            |           |       |         |         |         |         |           |             |
| **Push_Single**             | **1000000**   | **20.990 ms** | **22.4506 ms** | **1.2306 ms** |  **1.00** |    **0.00** |       **-** |       **-** |       **-** |  **64.84 KB** |        **1.00** |
| Push_Single_SlowBackend | 1000000   |  7.891 ms |  1.1378 ms | 0.0624 ms |  0.38 |    0.03 | 15.6250 | 15.6250 | 15.6250 |  64.84 KB |        1.00 |
