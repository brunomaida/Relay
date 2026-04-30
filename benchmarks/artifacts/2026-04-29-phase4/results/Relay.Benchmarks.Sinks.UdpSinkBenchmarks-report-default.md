
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

 Method      | ItemCount | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
------------ |---------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
 **Push_Single** | **100000**    | **4.078 ms** | **2.0361 ms** | **0.1116 ms** | **93.7500** | **93.7500** | **15.6250** |  **65.55 KB** |
 **Push_Single** | **1000000**   | **9.736 ms** | **0.2793 ms** | **0.0153 ms** | **93.7500** | **93.7500** | **15.6250** |  **65.56 KB** |
