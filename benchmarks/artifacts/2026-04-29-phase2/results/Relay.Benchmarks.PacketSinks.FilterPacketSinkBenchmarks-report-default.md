
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method               | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
--------------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
 Filter_Packet_Pass   | 3.2430 ns | 0.7783 ns | 0.0427 ns |  3.92 |    0.32 |     349 B |         - |          NA |
 Filter_Packet_Reject | 0.8305 ns | 1.3886 ns | 0.0761 ns |  1.00 |    0.00 |     526 B |         - |          NA |
