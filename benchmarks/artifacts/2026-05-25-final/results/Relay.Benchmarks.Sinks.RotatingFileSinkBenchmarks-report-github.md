```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error     | StdDev    | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|
| ShouldRotate_Predicate |  13.46 ns |  0.116 ns |  0.109 ns |         - |
| ShouldRotate_HotPath   | 134.69 ns | 22.116 ns | 65.211 ns |         - |
