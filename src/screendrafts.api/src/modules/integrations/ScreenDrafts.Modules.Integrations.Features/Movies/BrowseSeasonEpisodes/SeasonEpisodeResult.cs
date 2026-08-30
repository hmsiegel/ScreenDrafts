namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed record SeasonEpisodeResult
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
  public int SeasonNumber { get; init; }
  public int EpisodeNumber { get; init; }
  public string? AirDate { get; init; }
  public string? Overview { get; init; }
  public string? StillUrl { get; init; }
}
