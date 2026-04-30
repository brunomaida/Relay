```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

```
| Method      | ItemCount | Mean     | Error    | StdDev    | Allocated |
|------------ |---------- |---------:|---------:|----------:|----------:|
| **Push_Single** | **100000**    | **4.904 ms** | **1.890 ms** | **0.1036 ms** |  **18.38 KB** |
| **Push_Single** | **1000000**   | **6.865 ms** | **1.266 ms** | **0.0694 ms** |  **18.38 KB** |
