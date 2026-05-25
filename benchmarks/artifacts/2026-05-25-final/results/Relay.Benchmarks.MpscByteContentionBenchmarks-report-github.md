```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-JJPVIH : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                          | ProducerCount | Mean       | Error     | StdDev   | Allocated |
|-------------------------------- |-------------- |-----------:|----------:|---------:|----------:|
| **Mpsc_Byte_Throughput_TotalItems** | **1**             |   **572.8 ms** | **110.15 ms** | **316.0 ms** |     **464 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **2**             |   **640.7 ms** |  **97.22 ms** | **280.5 ms** |     **464 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **4**             |   **977.3 ms** | **116.28 ms** | **339.2 ms** |     **128 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **8**             | **1,165.7 ms** |  **96.78 ms** | **282.3 ms** |     **464 B** |
