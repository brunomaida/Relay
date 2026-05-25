```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-JJPVIH : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                      | ProducerCount | Capacity | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------------- |-------------- |--------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------------:|
| **StrideLayout_Throughput**     | **1**             | **1024**     |   **879.4 ms** | **154.53 ms** | **453.21 ms** |   **877.1 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 1024     | 1,265.3 ms | 214.25 ms | 631.73 ms | 1,074.0 ms |  2.35 |    2.93 |      88 B |        0.21 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **1**             | **65536**    |   **196.9 ms** |  **25.52 ms** |  **75.24 ms** |   **206.3 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 65536    |   214.7 ms |  22.72 ms |  66.99 ms |   216.9 ms |  1.31 |    0.77 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **1024**     | **1,103.9 ms** | **135.16 ms** | **392.12 ms** | **1,057.5 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 1024     | 1,207.8 ms | 245.04 ms | 722.52 ms | 1,058.3 ms |  1.23 |    1.01 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **65536**    |   **227.6 ms** |  **19.98 ms** |  **56.99 ms** |   **230.7 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 65536    |   305.9 ms |  22.89 ms |  67.48 ms |   291.8 ms |  1.46 |    0.58 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **1024**     | **1,573.9 ms** | **133.54 ms** | **380.99 ms** | **1,550.6 ms** |  **1.00** |    **0.00** |      **88 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 1024     | 1,738.5 ms | 185.62 ms | 544.39 ms | 1,661.9 ms |  1.19 |    0.46 |     424 B |        4.82 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **65536**    |   **384.8 ms** |  **25.45 ms** |  **74.63 ms** |   **383.2 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 65536    |   526.0 ms |  17.97 ms |  52.71 ms |   524.0 ms |  1.42 |    0.33 |     424 B |        1.00 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **1024**     | **1,512.4 ms** | **129.01 ms** | **376.34 ms** | **1,499.3 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 1024     | 1,573.5 ms | 150.85 ms | 444.79 ms | 1,522.8 ms |  1.12 |    0.45 |     136 B |        0.32 |
|                             |               |          |            |           |           |            |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **65536**    |   **631.5 ms** |  **26.77 ms** |  **78.93 ms** |   **624.3 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 65536    |   847.0 ms |  17.53 ms |  51.41 ms |   840.4 ms |  1.36 |    0.17 |     424 B |        1.00 |
