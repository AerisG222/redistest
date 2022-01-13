using BenchmarkDotNet.Running;
using redistest.Redis;
using redistest.Sql;

namespace redistest;

public class Program
{
    readonly PhotoRepository db = new PhotoRepository(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
    readonly PhotoCategoryCache redis = new PhotoCategoryCache("localhost:6379");

    public static async Task Main(string[] args)
    {
        var p = new Program();

        var runBenchmarks = args.Length > 0 && string.Equals(args[0], "y", StringComparison.InvariantCultureIgnoreCase) ? true : false;

        await p.ExecuteAsync(runBenchmarks);
    }

    async Task ExecuteAsync(bool runBenchmarks)
    {
        Console.WriteLine("retrieving all categories from sql...");
        var allRoles = new[] {"friend", "admin", "demo"};
        var categories = await db.GetCategoriesAsync(allRoles);

        Console.WriteLine("retrieving all roles from sql...");
        var categoryRoles = await db.GetCategoryRoles();

        Console.WriteLine("retrieving all photos from sql...");
        var photos = await GetPhotosAsync(categories, allRoles);

        Console.WriteLine("adding categories and roles to redis...");
        await redis.SetCategoriesAsync(categories, categoryRoles);

        Console.WriteLine("adding photos to redis...");
        await redis.SetPhotosAsync(photos, categoryRoles);

        Console.WriteLine("test of getting first 10 categories from redis:");
        var redisCategories = await redis.GetCategoriesAsync(new[] {"friend", "demo"});

        foreach(var cat in redisCategories.Take(10)) {
            Console.WriteLine($"    - {cat.Id} : {cat.Year} : {cat.Name}");
        }

        Console.WriteLine("test of getting first 10 photos from redis:");
        var redisPhotos = await redis.GetPhotosAsync(123, new[] {"friend", "demo"});

        foreach(var photo in redisPhotos.Take(10)) {
            Console.WriteLine($"    - {photo.Id} : {photo.CategoryId} : {photo.XsInfo.Path}");
        }

        Console.WriteLine("test of getting first 10 *random (naive)* photos from redis:");
        var redisRandomPhotos = await redis.GetRandomPhotosNaiveAsync(24, new[] {"friend", "demo"});

        foreach(var photo in redisRandomPhotos.Take(10)) {
            Console.WriteLine($"    - {photo.Id} : {photo.CategoryId} : {photo.XsInfo.Path}");
        }

        Console.WriteLine("test of getting first 10 *random (optimized)* photos from redis:");
        var redisRandomPhotosOptimized = await redis.GetRandomPhotosOptimizedAsync(24, new[] {"friend", "demo"});

        foreach(var photo in redisRandomPhotosOptimized.Take(10)) {
            Console.WriteLine($"    - {photo.Id} : {photo.CategoryId} : {photo.XsInfo.Path}");
        }

        if(runBenchmarks)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    async Task<IEnumerable<Photo>> GetPhotosAsync(IEnumerable<Category> categories, string[] roles)
    {
        var list = new List<Photo>();

        foreach(var category in categories) {
            list.AddRange(await db.GetPhotosForCategoryAsync(category.Id, roles));
        }

        return list;
    }
}
