namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

internal sealed class GetOnlineMovieCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<GetOnlineMovieCommand, GetOnlineMovieResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<GetOnlineMovieResponse>> Handle(GetOnlineMovieCommand command, CancellationToken cancellationToken)
  {
    var searchResult = await _tmdbService.FindMovieByImdbIdAsync(command.ImdbId, cancellationToken);

    if (searchResult is null)
    {
      return Result.Failure<GetOnlineMovieResponse>(MovieErrors.NotFound(command.ImdbId));
    }

    var detail = await _tmdbService.GetMovieDetailsAsync(searchResult.Id, cancellationToken);

    if (detail is null)
    {
      return Result.Failure<GetOnlineMovieResponse>(MovieErrors.NotFound(command.ImdbId));
    }

    var posterUri = _tmdbService.BuildPosterUrl(detail.PosterPath, "original");

    var directors = detail.Credits?.Crew?
      .Where(crew => crew.Job == "Director")
      .Select(crew => new DirectorModel(crew.Name, string.Empty, crew.Id))
      .ToList() ?? [];

    var writers = detail.Credits?.Crew?
      .Where(crew => crew.Job == "Writer")
      .Select(crew => new WriterModel(crew.Name, string.Empty, crew.Id))
      .ToList() ?? [];

    var producers = detail.Credits?.Crew?
      .Where(crew => crew.Job == "Producer")
      .Select(crew => new ProducerModel(crew.Name, string.Empty, crew.Id))
      .ToList() ?? [];

    var actors = detail.Credits?.Cast?
      .Select(cast => new ActorModel(cast.Name, string.Empty, cast.Id))
      .ToList() ?? [];

    var response = new GetOnlineMovieResponse(
      command.ImdbId,
      detail.Id,
      detail.Title,
      detail.ReleaseDate?[..4]!,
      detail.Overview,
      posterUri!.ToString(),
      detail.ReleaseDate,
      null,
      [],
      actors,
      directors,
      writers,
      producers,
      null!);

    return response;
  }
}
