```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8457)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method                | Mean     | Error     | StdDev    | Ratio | Code Size | Allocated | Alloc Ratio |
|---------------------- |---------:|----------:|----------:|------:|----------:|----------:|------------:|
| Multi_Packet_Enqueue  | 3.240 ns | 0.0395 ns | 0.0369 ns |  1.00 |     686 B |         - |          NA |
| Multi2_Packet_Enqueue | 4.042 ns | 0.0302 ns | 0.0283 ns |  1.25 |     458 B |         - |          NA |
