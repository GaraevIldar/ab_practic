namespace PracticalWork.Library.Abstractions.Services;

/// <summary>
/// Сервис кэширования данных
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Получение значения из кэша по ключу
    /// </summary>
    Task<T> GetAsync<T>(string key);

    /// <summary>
    /// Сохранение значения в кэш
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Удаление значения из кэша по ключу
    /// </summary>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Формирование составного ключа кэша из префикса, версии и параметров
    /// </summary>
    string GenerateCacheKey(string prefix, long cacheVersion, object parameters);

    /// <summary>
    /// Инвалидация кэша по ключу версии
    /// </summary>
    Task InvalidateCache(string cacheVersionKey);

    /// <summary>
    /// Получение текущей версии кэша
    /// </summary>
    Task<long> GetCurrentCacheVersion(string cacheVersionKey);
}