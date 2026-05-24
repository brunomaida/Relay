```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-HDGKLC : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                          | ProducerCount | Mean     | Error     | StdDev   | Allocated |
|-------------------------------- |-------------- |---------:|----------:|---------:|----------:|
| **Mpsc_Byte_Throughput_TotalItems** | **1**             | **537.1 ms** | **106.17 ms** | **304.6 ms** |     **464 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **2**             | **462.5 ms** |  **80.63 ms** | **231.3 ms** |     **464 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **4**             | **768.0 ms** |  **76.37 ms** | **221.6 ms** |     **464 B** |
| **Mpsc_Byte_Throughput_TotalItems** | **8**             | **828.3 ms** |  **69.34 ms** | **202.3 ms** |     **128 B** |
