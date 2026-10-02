namespace ScreenDrafts.Modules.Reporting.UnitTests.Doubles;

/// <summary>
/// Hand-written <see cref="ICacheService"/> that records every call so tests can assert
/// both what was touched and, just as importantly, what was left alone.
/// </summary>
internal sealed class RecordingCacheService : ICacheService
{
  public List<string> RemovedKeys { get; } = [];

  public List<string> Calls { get; } = [];

  public bool TryGetValue<T>(string key, out T? value)
  {
    Calls.Add($"TryGetValue:{key}");
    value = default;
    return false;
  }

  public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
  {
    Calls.Add($"Get:{key}");
    return Task.FromResult<T?>(default);
  }

  public Task SetAsync<T>(
    string key,
    T value,
    TimeSpan? expiration = null,
    CancellationToken cancellationToken = default
  )
  {
    Calls.Add($"Set:{key}");
    return Task.CompletedTask;
  }

  public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
  {
    Calls.Add($"Remove:{key}");
    RemovedKeys.Add(key);
    return Task.CompletedTask;
  }
}
