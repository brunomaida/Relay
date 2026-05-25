```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-QWWCSU : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
| Method                | Mean     | Error     | StdDev    | Allocated |
|---------------------- |---------:|----------:|----------:|----------:|
| FileSink_Enqueue_128B | 6.068 ns | 0.3190 ns | 0.0828 ns |         - |
