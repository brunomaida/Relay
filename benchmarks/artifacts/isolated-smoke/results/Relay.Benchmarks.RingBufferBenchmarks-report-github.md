```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-RFAFYJ : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

IterationCount=1  LaunchCount=1  WarmupCount=1  

```
| Method           | Capacity | Mean      | Error | Ratio | Code Size | Allocated | Alloc Ratio |
|----------------- |--------- |----------:|------:|------:|----------:|----------:|------------:|
| **TryConsume_Empty** | **64**       | **0.2161 ns** |    **NA** |  **1.00** |      **73 B** |         **-** |          **NA** |
|                  |          |           |       |       |           |           |             |
| **TryConsume_Empty** | **1024**     | **0.2173 ns** |    **NA** |  **1.00** |      **73 B** |         **-** |          **NA** |
|                  |          |           |       |       |           |           |             |
| **TryConsume_Empty** | **65536**    | **0.0152 ns** |    **NA** |  **1.00** |      **73 B** |         **-** |          **NA** |
