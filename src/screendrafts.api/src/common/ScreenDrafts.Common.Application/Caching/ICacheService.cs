namespace ScreenDrafts.Common.Application.Caching;

public interface ICacheService
{
  bool TryGetValue<T>(string key, out T? value);

  Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

  Task SetAsync<T>(
    string key,
    T value,
    TimeSpan? expiration = null,
    CancellationToken cancellationToken = default
  );

  Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
