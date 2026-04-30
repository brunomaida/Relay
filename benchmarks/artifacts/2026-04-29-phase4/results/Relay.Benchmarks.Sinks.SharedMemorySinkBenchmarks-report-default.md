
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method        | PayloadSize | Mean     | Error    | StdDev   | Code Size | Allocated |
-------------- |------------ |---------:|---------:|---------:|----------:|----------:|
 **Accept_Single** | **64**          | **11.79 ns** | **5.195 ns** | **0.285 ns** |   **1,783 B** |         **-** |
 **Accept_Single** | **256**         | **13.96 ns** | **7.888 ns** | **0.432 ns** |   **1,796 B** |         **-** |
