```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method               | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Healthy       | 0.0004 ns | 0.0017 ns | 0.0015 ns | 0.0000 ns |     ? |       ? |     148 B |         - |           ? |
| Depth2_AcceptReject  | 2.1527 ns | 0.0337 ns | 0.0315 ns | 2.1569 ns |     ? |       ? |     119 B |         - |           ? |
| Depth2_HeadUnhealthy | 0.8137 ns | 0.0032 ns | 0.0029 ns | 0.8133 ns |     ? |       ? |     135 B |         - |           ? |
| Depth3_AllUnhealthy  | 1.9661 ns | 0.0393 ns | 0.0368 ns | 1.9819 ns |     ? |       ? |     151 B |         - |           ? |
