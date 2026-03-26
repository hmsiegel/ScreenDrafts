namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

/// <summary>
/// Lightweight search result used for discovery.
/// </summary>
public sealed record TmdbSearchResult
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Overview { get; init; } = string.Empty;
  public string? PosterPath { get; init;  } = string.Empty;
  public string? ReleaseDate { get; init; }
}
