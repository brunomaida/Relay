```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method        | PayloadSize | Mean     | Error    | StdDev   | Code Size | Allocated |
|-------------- |------------ |---------:|---------:|---------:|----------:|----------:|
| **Accept_Single** | **64**          | **11.15 ns** | **0.073 ns** | **0.069 ns** |   **1,906 B** |         **-** |
| **Accept_Single** | **256**         | **12.02 ns** | **0.152 ns** | **0.127 ns** |   **1,881 B** |         **-** |
