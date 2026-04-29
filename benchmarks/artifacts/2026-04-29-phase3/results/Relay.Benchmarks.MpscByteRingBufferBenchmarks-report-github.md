```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                      | Capacity | PayloadSize | Mean       | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|---------------------------- |--------- |------------ |-----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| **Spsc_RoundTrip**              | **64**       | **8**           |  **3.1797 ns** | **0.3537 ns** | **0.0194 ns** |  **1.00** |    **0.00** |     **936 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 64       | 8           |  8.0443 ns | 4.4648 ns | 0.2447 ns |  2.53 |    0.09 |   1,424 B |         - |          NA |
| Mpsc_TryPublish_Full        | 64       | 8           |  2.4335 ns | 0.2823 ns | 0.0155 ns |  0.77 |    0.01 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 64       | 8           |  1.0780 ns | 2.2485 ns | 0.1232 ns |  0.34 |    0.04 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **64**       | **64**          |  **2.3375 ns** | **3.2651 ns** | **0.1790 ns** |  **1.00** |    **0.00** |     **794 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 64       | 64          |  3.4907 ns | 3.1868 ns | 0.1747 ns |  1.50 |    0.12 |   1,299 B |         - |          NA |
| Mpsc_TryPublish_Full        | 64       | 64          |  1.6544 ns | 1.7105 ns | 0.0938 ns |  0.71 |    0.08 |     750 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 64       | 64          |  0.2133 ns | 0.2904 ns | 0.0159 ns |  0.09 |    0.00 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **64**       | **256**         |  **0.8664 ns** | **0.8932 ns** | **0.0490 ns** |  **1.00** |    **0.00** |     **794 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 64       | 256         |  1.3718 ns | 0.2723 ns | 0.0149 ns |  1.59 |    0.09 |   1,299 B |         - |          NA |
| Mpsc_TryPublish_Full        | 64       | 256         |  0.6044 ns | 0.3330 ns | 0.0183 ns |  0.70 |    0.04 |     750 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 64       | 256         |  0.1982 ns | 0.1514 ns | 0.0083 ns |  0.23 |    0.02 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **64**       | **1024**        |  **0.8512 ns** | **0.5443 ns** | **0.0298 ns** |  **1.00** |    **0.00** |     **794 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 64       | 1024        |  1.4950 ns | 0.8023 ns | 0.0440 ns |  1.76 |    0.07 |   1,299 B |         - |          NA |
| Mpsc_TryPublish_Full        | 64       | 1024        |  0.6097 ns | 0.6564 ns | 0.0360 ns |  0.72 |    0.03 |     750 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 64       | 1024        |  0.1991 ns | 0.1978 ns | 0.0108 ns |  0.23 |    0.02 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **1024**     | **8**           |  **3.2285 ns** | **0.6216 ns** | **0.0341 ns** |  **1.00** |    **0.00** |     **936 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 1024     | 8           |  7.3194 ns | 0.2773 ns | 0.0152 ns |  2.27 |    0.02 |   1,424 B |         - |          NA |
| Mpsc_TryPublish_Full        | 1024     | 8           |  0.8134 ns | 0.7152 ns | 0.0392 ns |  0.25 |    0.01 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 1024     | 8           |  0.4057 ns | 0.4246 ns | 0.0233 ns |  0.13 |    0.01 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **1024**     | **64**          |  **3.5685 ns** | **0.8275 ns** | **0.0454 ns** |  **1.00** |    **0.00** |     **961 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 1024     | 64          |  8.5900 ns | 0.7465 ns | 0.0409 ns |  2.41 |    0.03 |   1,449 B |         - |          NA |
| Mpsc_TryPublish_Full        | 1024     | 64          |  0.8623 ns | 0.2735 ns | 0.0150 ns |  0.24 |    0.00 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 1024     | 64          |  0.4133 ns | 0.0542 ns | 0.0030 ns |  0.12 |    0.00 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **1024**     | **256**         |  **5.0176 ns** | **1.4741 ns** | **0.0808 ns** |  **1.00** |    **0.00** |     **938 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 1024     | 256         |  8.6944 ns | 1.5320 ns | 0.0840 ns |  1.73 |    0.04 |   1,405 B |         - |          NA |
| Mpsc_TryPublish_Full        | 1024     | 256         |  0.8315 ns | 0.2551 ns | 0.0140 ns |  0.17 |    0.01 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 1024     | 256         |  0.4025 ns | 0.1223 ns | 0.0067 ns |  0.08 |    0.00 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **1024**     | **1024**        |  **0.8398 ns** | **0.3928 ns** | **0.0215 ns** |  **1.00** |    **0.00** |     **794 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 1024     | 1024        |  1.3778 ns | 0.4480 ns | 0.0246 ns |  1.64 |    0.04 |   1,299 B |         - |          NA |
| Mpsc_TryPublish_Full        | 1024     | 1024        |  0.6124 ns | 0.1708 ns | 0.0094 ns |  0.73 |    0.03 |     750 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 1024     | 1024        |  0.1903 ns | 0.1534 ns | 0.0084 ns |  0.23 |    0.01 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **65536**    | **8**           |  **3.3329 ns** | **0.6010 ns** | **0.0329 ns** |  **1.00** |    **0.00** |     **936 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 65536    | 8           |  7.2446 ns | 0.5664 ns | 0.0310 ns |  2.17 |    0.03 |   1,424 B |         - |          NA |
| Mpsc_TryPublish_Full        | 65536    | 8           |  0.8120 ns | 0.3517 ns | 0.0193 ns |  0.24 |    0.01 |     751 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 65536    | 8           |  0.3984 ns | 0.2111 ns | 0.0116 ns |  0.12 |    0.00 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **65536**    | **64**          |  **3.5113 ns** | **0.4934 ns** | **0.0270 ns** |  **1.00** |    **0.00** |     **961 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 65536    | 64          |  7.7599 ns | 0.4621 ns | 0.0253 ns |  2.21 |    0.02 |   1,449 B |         - |          NA |
| Mpsc_TryPublish_Full        | 65536    | 64          |  0.8437 ns | 0.1582 ns | 0.0087 ns |  0.24 |    0.00 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 65536    | 64          |  0.3927 ns | 0.3264 ns | 0.0179 ns |  0.11 |    0.01 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **65536**    | **256**         |  **5.9473 ns** | **0.5232 ns** | **0.0287 ns** |  **1.00** |    **0.00** |     **934 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 65536    | 256         |  9.4995 ns | 1.9215 ns | 0.1053 ns |  1.60 |    0.01 |   1,422 B |         - |          NA |
| Mpsc_TryPublish_Full        | 65536    | 256         |  0.8296 ns | 0.6120 ns | 0.0335 ns |  0.14 |    0.01 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 65536    | 256         |  0.4200 ns | 0.3485 ns | 0.0191 ns |  0.07 |    0.00 |     515 B |         - |          NA |
|                             |          |             |            |           |           |       |         |           |           |             |
| **Spsc_RoundTrip**              | **65536**    | **1024**        | **15.3120 ns** | **1.1396 ns** | **0.0625 ns** |  **1.00** |    **0.00** |     **934 B** |         **-** |          **NA** |
| Mpsc_RoundTrip_NoContention | 65536    | 1024        | 19.1900 ns | 1.0649 ns | 0.0584 ns |  1.25 |    0.01 |   1,422 B |         - |          NA |
| Mpsc_TryPublish_Full        | 65536    | 1024        |  0.8618 ns | 0.7754 ns | 0.0425 ns |  0.06 |    0.00 |     767 B |         - |          NA |
| Mpsc_TryPeek_Empty          | 65536    | 1024        |  0.4183 ns | 0.1699 ns | 0.0093 ns |  0.03 |    0.00 |     515 B |         - |          NA |
