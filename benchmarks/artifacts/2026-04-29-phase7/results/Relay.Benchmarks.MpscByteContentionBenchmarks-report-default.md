
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

 Method                          | ProducerCount | Mean       | Error      | StdDev    | Allocated |
-------------------------------- |-------------- |-----------:|-----------:|----------:|----------:|
 **Mpsc_Byte_Throughput_TotalItems** | **1**             |   **459.1 ms** | **4,930.7 ms** | **270.27 ms** |     **800 B** |
 **Mpsc_Byte_Throughput_TotalItems** | **2**             |   **813.4 ms** | **8,230.8 ms** | **451.16 ms** |     **464 B** |
 **Mpsc_Byte_Throughput_TotalItems** | **4**             |   **660.8 ms** | **5,602.2 ms** | **307.08 ms** |     **128 B** |
 **Mpsc_Byte_Throughput_TotalItems** | **8**             | **1,116.8 ms** | **1,717.8 ms** |  **94.16 ms** |     **464 B** |
