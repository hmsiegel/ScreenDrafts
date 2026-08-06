namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed class GetPersonFilmographyCommandHandler(
  IImdbService imdbService,
  ITmdbService tmdbService
) : ICommandHandler<GetPersonFilmographyCommand, GetPersonFilmographyResponse>
{
  private readonly IImdbService _imdbService = imdbService;
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<GetPersonFilmographyResponse>> Handle(
    GetPersonFilmographyCommand request,
    CancellationToken cancellationToken
  )
  {
    var nameData = await _imdbService.GetPersonInformation(request.ImdbId);

    if (nameData is null || !string.IsNullOrEmpty(nameData.ErrorMessage))
    {
      return Result.Failure<GetPersonFilmographyResponse>(MovieErrors.NotFound(request.ImdbId));
    }

    var tmdbPersonId = await _tmdbService.FindPersonByImdbIdAsync(
      request.ImdbId,
      cancellationToken
    );

    if (tmdbPersonId is null)
    {
      return Result.Success(
        new GetPersonFilmographyResponse
        {
          PersonName = nameData.Name,
          PersonPhotoUrl = nameData.Image,
        }
      );
    }

    var credits = await _tmdbService.GetPersonCombinedCreditsAsync(
      tmdbPersonId.Value,
      cancellationToken
    );

    var mapped = credits
      .GroupBy(c => (c.TmdbId, c.MediaType))
      .Select(g => g.First())
      .Select(c => new FilmographyCreditResponse
      {
        TmdbId = c.TmdbId,
        Title = c.Title,
        Year = c.Year,
        PosterUrl = c.PosterUrl,
        MediaType = c.MediaType,
        CreditRole = c.CreditRole,
      })
      .OrderByDescending(c => c.Year)
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new GetPersonFilmographyResponse
      {
        PersonName = nameData.Name,
        PersonPhotoUrl = nameData.Image,
        Credits = mapped,
      }
    );
  }
}
