using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using redistest.Sql;
using redistest.Redis;

namespace redistest.Benchmarks;

[MinColumn, MaxColumn]
public class SqlVsRedisCategories
{
    readonly Consumer _consumer = new Consumer();
    readonly PhotoRepository _sql = new PhotoRepository(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
    readonly PhotoCategoryCache _redis = new PhotoCategoryCache("localhost:6379");
    readonly string[] _roles = new[] { "friend", "admin" };

    [Benchmark]
    public async Task QuerySql() => (await _sql.GetCategoriesAsync(_roles)).Consume(_consumer);

    [Benchmark]
    public async Task QueryRedis() => (await _redis.GetCategoriesAsync(_roles)).Consume(_consumer);
}