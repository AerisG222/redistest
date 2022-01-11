# redistest
This repo is a quick test to compare a sql join vs a redis join to explore benefits of using a redis caching layer.

Initial results:

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT


|     Method |     Mean |    Error |   StdDev |      Min |      Max |
|----------- |---------:|---------:|---------:|---------:|---------:|
|   QuerySql | 63.80 ms | 1.074 ms | 1.150 ms | 61.85 ms | 65.75 ms |
| QueryRedis | 25.46 ms | 0.497 ms | 0.592 ms | 24.57 ms | 26.73 ms |