
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method                | Mean     | Error     | StdDev    | Ratio | Code Size | Allocated | Alloc Ratio |
---------------------- |---------:|----------:|----------:|------:|----------:|----------:|------------:|
 Multi_Packet_Enqueue  | 3.207 ns | 0.9149 ns | 0.0501 ns |  1.00 |     690 B |         - |          NA |
 Multi2_Packet_Enqueue | 3.934 ns | 0.7591 ns | 0.0416 ns |  1.23 |     458 B |         - |          NA |
