// Feature: GET /integrations/movies/tv/seasons?seriesTmdbId={id}
// Lists a series' seasons so the episode picker can offer a dropdown instead
// of a typed season number. Sibling to BrowseSeasonEpisodes (which lists the
// episodes inside one season once the season is chosen).
//
// Depends on ITmdbService.GetTvSeasonsAsync, which does not exist yet.
namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseTvSeasons;

internal sealed class BrowseTvSeasonsCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<BrowseTvSeasonsCommand, BrowseTvSeasonsResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<BrowseTvSeasonsResponse>> Handle(
    BrowseTvSeasonsCommand request,
    CancellationToken cancellationToken
  )
  {
    var seasons = await _tmdbService.GetTvSeasonsAsync(request.SeriesTmdbId, cancellationToken);

    // TMDb's season 0 is the "Specials" bucket. useEpisodeBrowse rejects
    // seasonNumber < 1, so offering it would produce a dead option.
    var mapped = seasons
      .Where(s => s.SeasonNumber >= 1)
      .Select(s => new TvSeasonResult
      {
        SeasonNumber = s.SeasonNumber,
        Name = s.Name,
        EpisodeCount = s.EpisodeCount,
        AirDate = s.AirDate,
      })
      .OrderBy(s => s.SeasonNumber)
      .ToList()
      .AsReadOnly();

    if (mapped.Count == 0)
    {
      return Result.Failure<BrowseTvSeasonsResponse>(MovieErrors.NotFound(request.SeriesTmdbId));
    }

    var seriesTitle = await _tmdbService.GetTvShowNameAsync(
      request.SeriesTmdbId,
      cancellationToken
    );

    return Result.Success(
      new BrowseTvSeasonsResponse
      {
        SeriesTmdbId = request.SeriesTmdbId,
        SeriesTitle = seriesTitle,
        Seasons = mapped,
      }
    );
  }
}
