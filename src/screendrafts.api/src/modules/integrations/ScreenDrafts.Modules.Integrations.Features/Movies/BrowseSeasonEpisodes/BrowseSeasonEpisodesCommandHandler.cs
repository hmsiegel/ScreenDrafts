namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed class BrowseSeasonEpisodesCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<BrowseSeasonEpisodesCommand, BrowseSeasonEpisodesResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<BrowseSeasonEpisodesResponse>> Handle(
    BrowseSeasonEpisodesCommand request,
    CancellationToken cancellationToken
  )
  {
    var episodes = await _tmdbService.GetSeasonEpisodesAsync(
      request.SeriesTmdbId,
      request.SeasonNumber,
      cancellationToken
    );

    if (episodes.Count == 0)
    {
      return Result.Failure<BrowseSeasonEpisodesResponse>(
        MovieErrors.NotFound(request.SeriesTmdbId)
      );
    }

    var seriesTitle = await _tmdbService.GetTvShowNameAsync(
      request.SeriesTmdbId,
      cancellationToken
    );

    var mapped = episodes
      .Select(e => new SeasonEpisodeResult
      {
        TmdbId = e.Id,
        Name = e.Name,
        SeasonNumber = e.SeasonNumber,
        EpisodeNumber = e.EpisodeNumber,
        AirDate = e.AirDate,
        Overview = e.Overview,
        StillUrl = e.StillPath is not null
          ? _tmdbService.BuildPosterUrl(e.StillPath)?.ToString()
          : null,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new BrowseSeasonEpisodesResponse
      {
        SeriesTmdbId = request.SeriesTmdbId,
        SeriesTitle = seriesTitle,
        SeasonNumber = request.SeasonNumber,
        Episodes = mapped,
      }
    );
  }
}
