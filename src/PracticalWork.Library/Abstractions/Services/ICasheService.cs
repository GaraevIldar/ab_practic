namespace PracticalWork.Library.Abstractions.Services;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> RemoveAsync(string key);

    string GenerateCacheKey(string prefix, long cacheVersion, object parameters);
    Task InvalidateCache(string cacheVersionKey);
    Task<long> GetCurrentCacheVersion(string cacheVersionKey);
}