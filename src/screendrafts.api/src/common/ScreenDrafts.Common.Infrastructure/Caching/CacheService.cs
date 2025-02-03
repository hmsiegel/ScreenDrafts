﻿using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ScreenDrafts.Common.Infrastructure.Caching;

internal sealed class CacheService(IDistributedCache cache) : ICacheService
{
  private readonly IDistributedCache _cache = cache;

  public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
  {
    var bytes = await _cache.GetAsync(key, cancellationToken);

    return bytes is null ? default : Deserialize<T>(bytes);
  }

  public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
    _cache.RemoveAsync(key, cancellationToken);

  public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
  {
    var bytes = Serialize(value);

    return _cache.SetAsync(key, bytes, CacheOptions.Create(expiration), cancellationToken);
  }

  private static T Deserialize<T>(byte[] bytes)
  {
    return JsonSerializer.Deserialize<T>(bytes)!;
  }

  private static byte[] Serialize<T>(T value)
  {
    var buffer = new ArrayBufferWriter<byte>();

    using var writer = new Utf8JsonWriter(buffer);

    JsonSerializer.Serialize(writer, value);

    return buffer.WrittenSpan.ToArray();
  }
}

