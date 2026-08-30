namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed record BrowseSeasonEpisodesResponse
{
  public int SeriesTmdbId { get; init; }
  public string? SeriesTitle { get; init; }
  public int SeasonNumber { get; init; }
  public IReadOnlyList<SeasonEpisodeResult> Episodes { get; init; } = [];
}
