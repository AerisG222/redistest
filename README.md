# redistest
This repo is a quick test to compare a sql join vs a redis join to explore benefits of using a redis caching layer.

Initial results:

## Categories

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT

|     Method |     Mean |    Error |   StdDev |      Min |      Max |
|----------- |---------:|---------:|---------:|---------:|---------:|
|   QuerySql | 63.80 ms | 1.074 ms | 1.150 ms | 61.85 ms | 65.75 ms |
| QueryRedis | 25.46 ms | 0.497 ms | 0.592 ms | 24.57 ms | 26.73 ms |

## Photos in Category

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT


|     Method |     Mean |     Error |    StdDev |      Min |       Max |
|----------- |---------:|----------:|----------:|---------:|----------:|
|   QuerySql | 9.855 ms | 0.1947 ms | 0.1821 ms | 9.451 ms | 10.071 ms |
| QueryRedis | 4.557 ms | 0.0738 ms | 0.0820 ms | 4.407 ms |  4.710 ms |

## Random Photos (naive)

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT

|     Method |      Mean |    Error |   StdDev |       Min |       Max |
|----------- |----------:|---------:|---------:|----------:|----------:|
|   QuerySql | 146.30 ms | 1.348 ms | 1.261 ms | 143.13 ms | 148.25 ms |
| QueryRedis |  24.05 ms | 0.061 ms | 0.057 ms |  23.94 ms |  24.15 ms |

## Random Photos (optimized)

This performs some initial storage of which photos are accessible by which roles

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT

|     Method |      Mean |    Error |   StdDev |       Min |       Max |
|----------- |----------:|---------:|---------:|----------:|----------:|
|   QuerySql | 147.69 ms | 1.456 ms | 1.362 ms | 144.96 ms | 150.21 ms |
| QueryRedis |  20.89 ms | 0.228 ms | 0.190 ms |  20.65 ms |  21.34 ms |
