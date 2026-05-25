```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  Job-DJALFL : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
| Method               | Mean     | Error     | StdDev    | Allocated |
|--------------------- |---------:|----------:|----------:|----------:|
| TcpSink_Enqueue_128B | 6.808 ns | 0.7713 ns | 0.1194 ns |         - |
