```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method         | Mean     | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|--------------- |---------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Multi_Enqueue  | 3.064 ns | 0.0532 ns | 0.0498 ns |  1.00 |    0.00 |     291 B |         - |          NA |
| Multi2_Enqueue | 2.378 ns | 0.0490 ns | 0.0458 ns |  0.78 |    0.02 |     271 B |         - |          NA |
