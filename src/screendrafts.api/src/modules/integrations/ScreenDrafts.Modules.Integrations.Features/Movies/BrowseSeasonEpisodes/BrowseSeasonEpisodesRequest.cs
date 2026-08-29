namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed record BrowseSeasonEpisodesRequest
{
  [FromQuery(Name = "seriesTmdbId")]
  public int SeriesTmdbId { get; init; }

  [FromQuery(Name = "seasonNumber")]
  public int SeasonNumber { get; init; }
}
