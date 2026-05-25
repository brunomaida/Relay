```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-HDGKLC : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                      | ProducerCount | Capacity | Mean       | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------------- |-------------- |--------- |-----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **StrideLayout_Throughput**     | **1**             | **1024**     |   **496.2 ms** |  **77.80 ms** | **224.47 ms** |  **1.00** |    **0.00** |      **88 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 1024     |   566.2 ms |  81.68 ms | 239.54 ms |  1.58 |    1.53 |     424 B |        4.82 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **1**             | **65536**    |   **134.9 ms** |  **16.47 ms** |  **48.31 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 1             | 65536    |   188.8 ms |  19.14 ms |  55.84 ms |  1.65 |    0.94 |      88 B |        0.21 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **1024**     |   **719.1 ms** |  **73.86 ms** | **211.93 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 1024     | 1,035.5 ms |  99.62 ms | 292.18 ms |  1.54 |    0.61 |     424 B |        1.00 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **2**             | **65536**    |   **208.0 ms** |  **17.92 ms** |  **52.55 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 2             | 65536    |   251.1 ms |  15.06 ms |  43.45 ms |  1.28 |    0.40 |     424 B |        1.00 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **1024**     | **1,016.7 ms** |  **87.74 ms** | **255.95 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 1024     | 1,468.1 ms | 115.60 ms | 339.03 ms |  1.58 |    0.69 |     424 B |        1.00 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **4**             | **65536**    |   **322.4 ms** |  **20.61 ms** |  **59.80 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 4             | 65536    |   498.6 ms |  15.57 ms |  45.66 ms |  1.60 |    0.32 |     424 B |        1.00 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **1024**     | **1,351.7 ms** | **140.35 ms** | **413.82 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 1024     | 1,354.9 ms | 101.77 ms | 296.87 ms |  1.14 |    0.54 |      88 B |        0.21 |
|                             |               |          |            |           |           |       |         |           |             |
| **StrideLayout_Throughput**     | **8**             | **65536**    |   **571.0 ms** |  **24.85 ms** |  **72.88 ms** |  **1.00** |    **0.00** |     **424 B** |        **1.00** |
| LegacySlotLayout_Throughput | 8             | 65536    |   874.6 ms |  19.19 ms |  56.29 ms |  1.55 |    0.21 |     424 B |        1.00 |
