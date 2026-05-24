```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-GTQPYE : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
UnrollFactor=1  WarmupCount=3  

```
| Method                  | Scenario       | Mean     | Error    | StdDev   | Allocated |
|------------------------ |--------------- |---------:|---------:|---------:|----------:|
| **Measure_P999_Latency_Ns** | **Default**        | **96.43 ms** | **4.162 ms** | **0.644 ms** |  **17.32 KB** |
| **Measure_P999_Latency_Ns** | **NormalPriority** | **97.63 ms** | **3.239 ms** | **0.841 ms** |  **17.32 KB** |
| **Measure_P999_Latency_Ns** | **NormalPinned**   | **98.25 ms** | **2.835 ms** | **0.736 ms** |  **17.32 KB** |
