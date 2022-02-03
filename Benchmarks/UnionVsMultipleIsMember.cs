using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using redistest.Sql;
using redistest.Redis;

namespace redistest.Benchmarks;

[ShortRunJob]
[MinColumn, MaxColumn]
public class UnionVsMultipleIsMember
{
    readonly Consumer _consumer = new();
    readonly PhotoRepository _sql = new(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
    readonly PhotoCategoryCache _redis = new("localhost:6379,allowAdmin=true");
    readonly string[] _roles = new[] { "friend", "demo" };

    [Benchmark]
    public Task CheckAccessViaUnion() => _redis.CanAccessViaUnion(123, _roles);

    [Benchmark]
    public Task CheckAccessViaMultipleIsMemberCalls() => _redis.CanAccessViaMultipleIsMember(123, _roles);
}