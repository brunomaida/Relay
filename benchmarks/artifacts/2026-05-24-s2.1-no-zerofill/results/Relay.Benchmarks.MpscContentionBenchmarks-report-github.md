```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-JTSQJH : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                     | ProducerCount | Mean     | Error    | StdDev   | Allocated |
|--------------------------- |-------------- |---------:|---------:|---------:|----------:|
| **Mpsc_Throughput_TotalItems** | **1**             | **166.6 ms** | **22.52 ms** | **66.05 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **2**             | **261.9 ms** | **19.81 ms** | **57.80 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **4**             | **515.4 ms** | **17.48 ms** | **50.71 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **8**             | **825.9 ms** | **20.33 ms** | **59.64 ms** |     **464 B** |
