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

## Batch Inserts

This test was to see the impact of loading records into redis in batches

BenchmarkDotNet=v0.13.1, OS=fedora 35
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52901), X64 RyuJIT

Job=ShortRun  InvocationCount=1  IterationCount=3
LaunchCount=1  UnrollFactor=1  WarmupCount=3

|     Method | BatchSize |     Mean |     Error |   StdDev |      Min |      Max |
|----------- |---------- |---------:|----------:|---------:|---------:|---------:|
| BulkInsert |         1 | 11.284 s | 18.6204 s | 1.0206 s | 10.229 s | 12.266 s |
| BulkInsert |         2 |  7.774 s |  6.2482 s | 0.3425 s |  7.530 s |  8.166 s |
| BulkInsert |         5 |  5.152 s |  3.9921 s | 0.2188 s |  4.954 s |  5.387 s |
| BulkInsert |        10 |  4.082 s |  3.6275 s | 0.1988 s |  3.858 s |  4.238 s |
| BulkInsert |        20 |  3.713 s |  2.1289 s | 0.1167 s |  3.631 s |  3.847 s |
| BulkInsert |        50 |  3.293 s |  3.3800 s | 0.1853 s |  3.090 s |  3.454 s |
| BulkInsert |       100 |  3.017 s |  0.8747 s | 0.0479 s |  2.965 s |  3.060 s |
| BulkInsert |       250 |  3.112 s |  0.7158 s | 0.0392 s |  3.084 s |  3.157 s |
| BulkInsert |       500 |  2.812 s |  5.3167 s | 0.2914 s |  2.478 s |  3.017 s |
| BulkInsert |      1000 |  2.476 s |  0.3428 s | 0.0188 s |  2.456 s |  2.492 s |
| BulkInsert |     10000 |  2.651 s |  0.1235 s | 0.0068 s |  2.647 s |  2.659 s |
| BulkInsert |   1000000 |  2.766 s |  0.2366 s | 0.0130 s |  2.756 s |  2.781 s |
