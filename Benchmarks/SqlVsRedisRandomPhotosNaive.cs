using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using redistest.Sql;
using redistest.Redis;

namespace redistest.Benchmarks;

[MinColumn, MaxColumn]
public class SqlVsRedisRandomPhotosNaive
{
    readonly Consumer _consumer = new Consumer();
    readonly PhotoRepository _sql = new PhotoRepository(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
    readonly PhotoCategoryCache _redis = new PhotoCategoryCache("localhost:6379");
    readonly string[] _roles = new[] { "friend", "demo" };

    [Benchmark]
    public async Task QuerySql() => (await _sql.GetRandomAsync(24, _roles)).Consume(_consumer);

    [Benchmark]
    public async Task QueryRedis() => (await _redis.GetRandomPhotosNaiveAsync(24, _roles)).Consume(_consumer);
}