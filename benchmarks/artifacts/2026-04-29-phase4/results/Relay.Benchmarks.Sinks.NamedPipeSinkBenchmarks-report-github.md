```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method      | ItemCount | Mean       | Error      | StdDev   | Gen0     | Gen1     | Gen2    | Allocated |
|------------ |---------- |-----------:|-----------:|---------:|---------:|---------:|--------:|----------:|
| **Push_Single** | **10000**     |   **501.6 μs** | **1,457.7 μs** | **79.90 μs** | **223.1445** | **223.1445** | **37.5977** | **134.46 KB** |
| **Push_Single** | **100000**    | **1,068.8 μs** |   **185.8 μs** | **10.18 μs** | **212.8906** | **212.8906** | **37.1094** | **134.46 KB** |
