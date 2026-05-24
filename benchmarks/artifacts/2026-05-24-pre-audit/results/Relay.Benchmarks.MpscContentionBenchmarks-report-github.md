```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-HDGKLC : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                     | ProducerCount | Mean     | Error    | StdDev   | Allocated |
|--------------------------- |-------------- |---------:|---------:|---------:|----------:|
| **Mpsc_Throughput_TotalItems** | **1**             | **175.1 ms** | **21.70 ms** | **63.64 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **2**             | **238.6 ms** | **20.83 ms** | **60.43 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **4**             | **352.6 ms** | **22.27 ms** | **64.95 ms** |     **128 B** |
| **Mpsc_Throughput_TotalItems** | **8**             | **551.3 ms** | **15.22 ms** | **44.65 ms** |     **464 B** |
