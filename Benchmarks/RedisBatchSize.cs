using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using redistest.Sql;
using redistest.Redis;

namespace redistest.Benchmarks;

[ShortRunJob]
[MinColumn, MaxColumn]
public class RedisBatchSize
{
    readonly Consumer _consumer = new();
    readonly PhotoRepository _sql = new(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
    readonly PhotoCategoryCache _redis = new("localhost:6379,allowAdmin=true");
    readonly string[] _roles = new[] { "friend", "demo" };
    List<Photo> _photos = null!;

    [Params(1, 2, 5, 10, 20, 50, 100, 250, 500, 1_000, 10_000, 1_000_000)]
    public int BatchSize { get; set; }

    [IterationSetup]
    public void PrepareTestAsync()
    {
        _redis.FlushDb().Wait();

        if(_photos == null)
        {
            List<Photo> photos = new();
            var categories = _sql.GetCategoriesAsync(_roles).Result;

            foreach(var category in categories)
            {
                photos.AddRange(_sql.GetPhotosForCategoryAsync(category.Id, _roles).Result);
            }

            _photos = photos;
        }
    }

    [Benchmark]
    public Task BulkInsert() => _redis.BulkAddPhotoPerformanceTestAsync(_photos, BatchSize);
}