// Feature: GET /integrations/movies/tv/seasons?seriesTmdbId={id}
// Lists a series' seasons so the episode picker can offer a dropdown instead
// of a typed season number. Sibling to BrowseSeasonEpisodes (which lists the
// episodes inside one season once the season is chosen).
//
// Depends on ITmdbService.GetTvSeasonsAsync, which does not exist yet.
namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseTvSeasons;

internal sealed record TvSeasonResult
{
  public int SeasonNumber { get; init; }
  public string Name { get; init; } = string.Empty;
  public int EpisodeCount { get; init; }
  public string? AirDate { get; init; }
}
