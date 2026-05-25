```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method               | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|----------:|------------:|
| TcpSinkTyped_Enqueue | 0.3309 ns | 0.0038 ns | 0.0034 ns |  1.00 |         - |          NA |
