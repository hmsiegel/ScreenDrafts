namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed record BrowseSeasonEpisodesCommand : ICommand<BrowseSeasonEpisodesResponse>
{
  public int SeriesTmdbId { get; init; }
  public int SeasonNumber { get; init; }
}
