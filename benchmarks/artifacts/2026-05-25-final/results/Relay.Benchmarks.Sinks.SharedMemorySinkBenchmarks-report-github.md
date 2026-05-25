```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method        | PayloadSize | Mean     | Error     | StdDev    | Code Size | Allocated |
|-------------- |------------ |---------:|----------:|----------:|----------:|----------:|
| **Accept_Single** | **64**          | **6.835 ns** | **0.0848 ns** | **0.0793 ns** |   **2,179 B** |         **-** |
| **Accept_Single** | **256**         | **9.726 ns** | **0.2236 ns** | **0.2907 ns** |   **2,154 B** |         **-** |
