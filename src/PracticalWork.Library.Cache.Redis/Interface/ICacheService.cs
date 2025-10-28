#nullable enable
namespace PracticalWork.Library.Cache.Redis;

public interface ICacheService
{
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    public Task<T?> GetAsync<T>(string key);
}