namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

/// <summary>
/// Full details returned for any media type (movie, TV show, TV episode).
/// Credits are always populated where available.
/// </summary>
public sealed record TmdbMediaDetails
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Overview { get; init; } = string.Empty;
  public string? PosterPath { get; init; }
  public string? ReleaseDate { get; init; }
  public Uri? TrailerUrl { get; init; }
  public IReadOnlyList<TmdbGenre> Genres { get; init; } = [];
  public TmdbCredits Credits { get; init; } = default!;

  // TV Episode specific fields
  public int? TVSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}
