
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method           | Mean     | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
----------------- |---------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
 Multi_IsHealthy  | 1.132 ns | 0.5472 ns | 0.0300 ns |  1.00 |    0.00 |      95 B |         - |          NA |
 Multi2_IsHealthy | 1.079 ns | 1.1591 ns | 0.0635 ns |  0.95 |    0.03 |      84 B |         - |          NA |
