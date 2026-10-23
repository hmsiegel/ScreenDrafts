namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Spotify;

/// <summary>
/// Holds the client-credentials token between requests. Registered as a
/// singleton because <see cref="SpotifyService"/> is transient (typed
/// HttpClient). Two concurrent misses may each fetch a token; the last write
/// wins, which is harmless.
/// </summary>
internal sealed class SpotifyTokenCache
{
  private static readonly TimeSpan _expiryMargin = TimeSpan.FromMinutes(1);

  private AccessToken? _current;

  public string? TryGet()
  {
    var current = _current;

    return current is not null && current.ExpiresAt > TimeProvider.System.GetUtcNow()
      ? current.Value
      : null;
  }

  public void Set(string value, TimeSpan lifetime) =>
    _current = new AccessToken(
      value,
      TimeProvider.System.GetUtcNow() + lifetime - _expiryMargin
    );

  public void Clear() => _current = null;

  private sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
}
