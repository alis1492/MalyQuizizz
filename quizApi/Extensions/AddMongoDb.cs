using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace quizApi.Extensions;

public static class MongoDbExtensions
{
    // Add a new client Singleton using a uri from environment variable
    public static IServiceCollection AddMongoClient(this IServiceCollection services)
    {
        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
        var client = new MongoClient(Environment.GetEnvironmentVariable("ATLAS_URI"));
        services.AddSingleton(client);
        return services;
    }
    // Add a new database Singleton from a given name
    public static IServiceCollection AddDatabase(this IServiceCollection services, string dbName = "quizApi")
    {
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<MongoClient>();
            var db = client.GetDatabase(dbName);
            return db;
        });
        return services;
    }
    // Add a new collection Singleton from a given name
    public static IServiceCollection AddCollection<T>(this IServiceCollection services, string collectionName)
    {
        services.AddSingleton<IMongoCollection<T>>(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            var collection = db.GetCollection<T>(collectionName);
            return collection;
        });
        return services;
    }
}