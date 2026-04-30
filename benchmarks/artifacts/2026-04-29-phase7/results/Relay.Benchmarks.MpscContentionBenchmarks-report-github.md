```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

```
| Method                     | ProducerCount | Mean      | Error       | StdDev   | Allocated |
|--------------------------- |-------------- |----------:|------------:|---------:|----------:|
| **Mpsc_Throughput_TotalItems** | **1**             |  **94.73 ms** |   **591.01 ms** | **32.40 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **2**             | **258.45 ms** | **1,599.86 ms** | **87.69 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **4**             | **325.16 ms** |   **885.38 ms** | **48.53 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **8**             | **602.23 ms** | **1,620.48 ms** | **88.82 ms** |     **464 B** |
