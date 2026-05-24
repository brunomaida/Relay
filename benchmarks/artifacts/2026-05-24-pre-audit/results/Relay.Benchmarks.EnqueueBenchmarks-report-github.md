```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method               | Mean      | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Depth1_Healthy       | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |     148 B |         - |           ? |
| Depth2_AcceptReject  | 1.9345 ns | 0.0384 ns | 0.0359 ns |     ? |       ? |     142 B |         - |           ? |
| Depth2_HeadUnhealthy | 0.8412 ns | 0.0093 ns | 0.0083 ns |     ? |       ? |     135 B |         - |           ? |
| Depth3_AllUnhealthy  | 2.0296 ns | 0.0330 ns | 0.0309 ns |     ? |       ? |     150 B |         - |           ? |
