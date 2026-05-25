```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method         | Mean     | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|--------------- |---------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Multi_Enqueue  | 3.396 ns | 0.0924 ns | 0.0864 ns |  1.00 |    0.00 |     295 B |         - |          NA |
| Multi2_Enqueue | 2.406 ns | 0.0645 ns | 0.0768 ns |  0.72 |    0.03 |     271 B |         - |          NA |
