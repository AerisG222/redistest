using BenchmarkDotNet.Running;
using redistest.Redis;
using redistest.Sql;

Console.WriteLine("retrieving all categories and roles from sql...");

var db = new PhotoRepository(Environment.GetEnvironmentVariable("MAW_API_Environment__DbConnectionString"));
var categories = await db.GetCategoriesAsync(new[] {"friend", "admin", "demo"});
var categoryRoles = await db.GetCategoryRoles();

var redis = new PhotoCategoryCache("localhost:6379");

Console.WriteLine("adding categories and roles to redis...");

await redis.SetCategoriesAsync(categories, categoryRoles);

Console.WriteLine("test of getting first 10 categories from redis:");

var redisCategories = await redis.GetCategoriesAsync(new[] {"friend", "demo"});

foreach(var cat in redisCategories.Take(10)) {
    Console.WriteLine($"    - {cat.Id} : {cat.Year} : {cat.Name}");
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
