```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error     | StdDev    | Median    | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|----------:|
| ShouldRotate_Predicate |  13.20 ns |  0.024 ns |  0.022 ns |  13.20 ns |         - |
| ShouldRotate_HotPath   | 129.68 ns | 19.689 ns | 58.055 ns | 105.46 ns |         - |
