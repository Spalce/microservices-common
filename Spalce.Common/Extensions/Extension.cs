using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Spalce.Common.Classes;
using Spalce.Common.Repositories;
using Spalce.Common.Settings;

namespace Spalce.Common.Extensions;

public static class Extensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new TimeSpanSerializer(BsonType.String));
        services.AddSingleton(sp =>
        {
            var configuration  = sp.GetRequiredService<IConfiguration>();
            var serviceSetting = configuration.GetSection(nameof(ServiceSetting)).Get<ServiceSetting>();
            var mongoDb = configuration.GetSection(nameof(MongoDbSetting)).Get<MongoDbSetting>();
            var mongoClient = new MongoClient(mongoDb.ConnectionString);
            return mongoClient.GetDatabase(serviceSetting.Name);
        });

        return services;
    }

    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
    {
        services.AddSingleton<IRepository<T>>(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            return new MongoRepository<T>(db, collectionName);
        });

        return services;
    }
}
