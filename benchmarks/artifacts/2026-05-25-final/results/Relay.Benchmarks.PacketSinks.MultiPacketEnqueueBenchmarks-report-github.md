```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                | Mean     | Error     | StdDev    | Ratio | RatioSD | Code Size | Allocated | Alloc Ratio |
|---------------------- |---------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| Multi_Packet_Enqueue  | 3.303 ns | 0.0323 ns | 0.0302 ns |  1.00 |    0.00 |     688 B |         - |          NA |
| Multi2_Packet_Enqueue | 4.272 ns | 0.0565 ns | 0.0500 ns |  1.29 |    0.02 |     458 B |         - |          NA |
