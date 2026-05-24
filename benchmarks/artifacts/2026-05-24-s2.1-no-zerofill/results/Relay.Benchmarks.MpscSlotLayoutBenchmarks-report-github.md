```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-JTSQJH : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                      | ProducerCount | Capacity | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------------- |-------------- |--------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------------:|
| **StrideLayout_Throughput**     | **1**             | **1024**     |   **484.2 ms** |  **71.59 ms** | **206.56 ms** |   **459.9 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 1024     |   505.9 ms |  73.97 ms | 216.93 ms |   448.9 ms |  1.26 |    0.82 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **1**             | **65536**    |   **143.6 ms** |  **18.22 ms** |  **53.16 ms** |   **127.1 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 65536    |   135.8 ms |  18.42 ms |  53.44 ms |   122.1 ms |  1.08 |    0.69 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **1024**     |   **628.3 ms** |  **66.88 ms** | **194.04 ms** |   **611.8 ms** |  **1.00** |    **0.00** |      **88 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 1024     |   763.5 ms |  79.55 ms | 229.51 ms |   737.8 ms |  1.37 |    0.77 |     424 B |        4.82 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **65536**    |   **206.5 ms** |  **17.41 ms** |  **50.23 ms** |   **205.0 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 65536    |   243.9 ms |  11.57 ms |  33.02 ms |   241.5 ms |  1.24 |    0.35 |      88 B |        0.21 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **1024**     | **1,102.3 ms** |  **91.03 ms** | **265.55 ms** | **1,124.3 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 1024     | 1,245.6 ms | 107.70 ms | 315.85 ms | 1,227.9 ms |  1.22 |    0.47 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **65536**    |   **289.2 ms** |  **16.05 ms** |  **47.08 ms** |   **288.8 ms** |  **1.00** |    **0.00** |      **88 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 65536    |   495.5 ms |  13.50 ms |  39.38 ms |   493.4 ms |  1.76 |    0.33 |     424 B |        4.82 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **1024**     | **1,361.1 ms** | **109.56 ms** | **321.33 ms** | **1,348.7 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 1024     | 1,245.1 ms |  85.03 ms | 248.02 ms | 1,197.3 ms |  0.97 |    0.32 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **65536**    |   **487.5 ms** |  **14.35 ms** |  **41.41 ms** |   **487.9 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 65536    |   861.1 ms |  21.16 ms |  61.05 ms |   863.8 ms |  1.78 |    0.19 |      88 B |        0.21 |
