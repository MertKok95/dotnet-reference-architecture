using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotNetCoreLab.Infrastructure.Persistence;
using DotNetCoreLab.Infrastructure.Repositories;
using DotNetCoreLab.Domain.Interfaces;
using DotNetCoreLab.Infrastructure.Repositories.EfCore;
using DotNetCoreLab.Infrastructure.Repositories.Mongo;
using DotNetCoreLab.Infrastructure.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Infrastructure.Messaging;
using DotNetCoreLab.Infrastructure.Security;
using DotNetCoreLab.Infrastructure.Services;

namespace DotNetCoreLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<ITokenService, TokenService>();


        var dbProvider = configuration["DatabaseProvider"] ?? "SQLite";

        if (dbProvider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase) ||
            dbProvider.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch
            {
                // Kayıtlıysa hata fırlatma durumunda ele alınacak alan
            }

            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var connString = !string.IsNullOrWhiteSpace(settings.ConnectionString)
                    ? settings.ConnectionString
                    : "mongodb://localhost:27017";
                return new MongoClient(connString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                var dbName = !string.IsNullOrWhiteSpace(settings.DatabaseName)
                    ? settings.DatabaseName
                    : "DotNetCoreLabDb";
                return client.GetDatabase(dbName);
            });

            services.AddScoped<IProductRepository, MongoProductRepository>();
        }
        else
        {
            services.AddScoped<IProductRepository, EfProductRepository>();
        }

        // --- REDIS DAĞITIK ÖNBELLEK (DISTRIBUTED CACHE) ---
        var useRedis = configuration.GetValue<bool>("UseRedis");

        if (useRedis)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379,abortConnect=false";
                options.InstanceName = "DotNetCoreLab_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // --- MESAJ KUYRUĞU (RABBITMQ VEYA AWS SQS) ---
        var messageBroker = configuration["MessageBroker"] ?? "RabbitMQ";

        if (messageBroker.Equals("AWS_SQS", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IMessagePublisher, AwsSqsPublisher>();
            services.AddHostedService<AwsSqsConsumer>();
        }
        else
        {
            services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
            services.AddHostedService<ProductCreatedConsumer>();
        }

        // --- ELASTICSEARCH ARAMA MOTORU ---
        services.AddHttpClient<IProductSearchService, ElasticsearchService>();

        // --- AWS S3 BULUT DOSYA DEPOLAMA SERVİSİ ---
        services.AddScoped<IFileStorageService, AwsS3StorageService>();

        return services;
    }
}