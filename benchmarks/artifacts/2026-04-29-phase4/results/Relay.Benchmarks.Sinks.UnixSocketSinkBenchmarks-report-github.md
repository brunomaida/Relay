```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method      | ItemCount | Mean      | Error     | StdDev    | Gen0     | Gen1     | Gen2    | Allocated |
|------------ |---------- |----------:|----------:|----------:|---------:|---------:|--------:|----------:|
| **Push_Single** | **100000**    |  **2.593 ms** | **15.332 ms** | **0.8404 ms** | **222.6563** | **222.6563** | **37.1094** | **135.84 KB** |
| **Push_Single** | **1000000**   | **11.884 ms** | **40.428 ms** | **2.2160 ms** | **187.5000** | **187.5000** | **31.2500** | **135.85 KB** |
