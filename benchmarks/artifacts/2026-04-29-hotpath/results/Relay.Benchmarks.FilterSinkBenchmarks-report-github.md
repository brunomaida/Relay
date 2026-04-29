```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8246)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]   : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method        | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|-------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Filter_Pass   | 6.2172 ns | 1.5490 ns | 0.0849 ns |  7.57 |    0.46 |     683 B |         - |          NA |
| Filter_Reject | 0.8234 ns | 1.0096 ns | 0.0553 ns |  1.00 |    0.00 |     385 B |         - |          NA |
