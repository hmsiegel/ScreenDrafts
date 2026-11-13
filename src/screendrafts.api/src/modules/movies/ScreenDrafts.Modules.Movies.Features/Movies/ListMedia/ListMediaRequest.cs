namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed record ListMediaRequest
{
  [FromQuery]
  public int Page { get; init; } = 1;

  [FromQuery]
  public int PageSize { get; init; } = 24;

  [FromQuery]
  public string? Search { get; init; }

  /// <summary>
  /// Integer value of MediaType SmartEnum: 0=Movie, 1=TvShow, 2=TvEpisode, 3=VideoGame, 4=MusicVideo
  /// </summary>
  [FromQuery]
  public int? MediaType { get; init; }

  /// <summary>
  /// Four-digit year string, e.g. "1972". Matches prefix so "1972" matches "1972–1974" series.
  /// </summary>
  [FromQuery]
  public string? Year { get; init; }

  /// <summary>
  /// Sort order: title_asc (default), title_desc, year_asc, year_desc
  /// </summary>
  [FromQuery]
  public string? Sort { get; init; }

  /// <summary>
  /// Person key from GET /media/people: "t:{tmdbId}" or "i:{imdbId}". Limits results to
  /// titles this person is credited on, in the given Role.
  /// </summary>
  [FromQuery]
  public string? PersonKey { get; init; }

  /// <summary>
  /// Credit role for PersonTmdbId: any (default), actor, director, writer, producer.
  /// Ignored when PersonKey is not set.
  /// </summary>
  [FromQuery]
  public string? Role { get; init; }
}
