```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-JJPVIH : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                     | ProducerCount | Mean     | Error    | StdDev   | Allocated |
|--------------------------- |-------------- |---------:|---------:|---------:|----------:|
| **Mpsc_Throughput_TotalItems** | **1**             | **207.8 ms** | **25.74 ms** | **75.07 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **2**             | **264.3 ms** | **25.75 ms** | **75.10 ms** |     **464 B** |
| **Mpsc_Throughput_TotalItems** | **4**             | **334.5 ms** | **18.87 ms** | **55.65 ms** |     **128 B** |
| **Mpsc_Throughput_TotalItems** | **8**             | **683.6 ms** | **32.96 ms** | **97.18 ms** |     **176 B** |
