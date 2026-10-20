namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

/// <summary>A TMDb person record resolved from an IMDb person ID.</summary>
public sealed record TmdbPersonProfile
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
#pragma warning disable CA1056 // URI-like properties should not be strings
  public string? PhotoUrl { get; init; }
#pragma warning restore CA1056 // URI-like properties should not be strings
}
