
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method                      | ItemCount | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
---------------------------- |---------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
 **Push_Single**                 | **100000**    |   **1.532 ms** |  **1.355 ms** | **0.0743 ms** |  **1.00** |    **0.00** | **3.9063** | **3.9063** | **3.9063** |  **16.92 KB** |        **1.00** |
 Push_Batch32                | 100000    |   1.584 ms |  1.273 ms | 0.0698 ms |  1.03 |    0.01 | 3.9063 | 3.9063 | 3.9063 |  16.92 KB |        1.00 |
 Push_Single_SlowBackend     | 100000    | 117.408 ms | 11.762 ms | 0.6447 ms | 76.76 |    3.86 |      - |      - |      - |  16.93 KB |        1.00 |
 Push_Batch32_SlowBackend    | 100000    | 118.195 ms | 23.657 ms | 1.2967 ms | 77.25 |    3.16 |      - |      - |      - |  16.99 KB |        1.00 |
 MpscPush_Single             | 100000    |   5.684 ms |  3.559 ms | 0.1951 ms |  3.72 |    0.26 |      - |      - |      - |  16.81 KB |        0.99 |
 MpscPush_Single_SlowBackend | 100000    | 118.217 ms | 35.396 ms | 1.9402 ms | 77.29 |    4.19 |      - |      - |      - |  16.88 KB |        1.00 |
                             |           |            |           |           |       |         |        |        |        |           |             |
 **Push_Single**                 | **1000000**   |   **3.830 ms** |  **3.340 ms** | **0.1831 ms** |  **1.00** |    **0.00** | **3.9063** | **3.9063** | **3.9063** |  **16.92 KB** |        **1.00** |
 Push_Batch32                | 1000000   |   2.702 ms |  1.625 ms | 0.0891 ms |  0.71 |    0.06 | 3.9063 | 3.9063 | 3.9063 |  16.92 KB |        1.00 |
 Push_Single_SlowBackend     | 1000000   | 117.194 ms |  9.637 ms | 0.5282 ms | 30.65 |    1.32 |      - |      - |      - |  16.99 KB |        1.00 |
 Push_Batch32_SlowBackend    | 1000000   | 116.376 ms |  2.566 ms | 0.1406 ms | 30.44 |    1.47 |      - |      - |      - |  16.99 KB |        1.00 |
 MpscPush_Single             | 1000000   |  16.071 ms | 13.672 ms | 0.7494 ms |  4.20 |    0.09 |      - |      - |      - |  16.82 KB |        0.99 |
 MpscPush_Single_SlowBackend | 1000000   | 117.676 ms | 10.841 ms | 0.5942 ms | 30.77 |    1.33 |      - |      - |      - |  16.88 KB |        1.00 |
