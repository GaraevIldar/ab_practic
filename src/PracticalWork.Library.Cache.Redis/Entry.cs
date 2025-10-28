using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PracticalWork.Library.Cache.Redis;

public static class Entry
{
    // summary>
    // Регистрация зависимостей для распределенного Cache
    // </summary>
    // public static IServiceCollection AddCache(this IServiceCollection serviceCollection, IConfiguration configuration)
    // {
    //     var connectionString = configuration["App:Redis:RedisCacheConnection"];
    //     var prefix = configuration["App:Redis:RedisCachePrefix"];
    //
    //     // Реализация подключения к Redis и сервисов
    //
    //     if (string.IsNullOrWhiteSpace(connectionString))
    //         throw new InvalidOperationException("Redis connection string (App:Redis:RedisCacheConnection) не указана в конфигурации.");
    //
    //     // Подключаем Redis как распределённый кэш
    //     serviceCollection.AddStackExchangeRedisCache(options =>
    //     {
    //         options.Configuration = connectionString;
    //         options.InstanceName = prefix ?? "AppCache:"; // Префикс ключей
    //     });
    //     
    //     serviceCollection.AddSingleton<ICacheService, RedisCacheService>();
    //     
    //     return serviceCollection;
    // }
}

