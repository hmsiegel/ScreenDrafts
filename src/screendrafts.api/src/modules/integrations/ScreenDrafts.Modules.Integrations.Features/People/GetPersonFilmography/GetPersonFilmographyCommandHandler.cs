namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed class GetPersonFilmographyCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<GetPersonFilmographyCommand, GetPersonFilmographyResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<GetPersonFilmographyResponse>> Handle(
    GetPersonFilmographyCommand request,
    CancellationToken cancellationToken
  )
  {
    // Name and photo come from TMDb: the IMDb API no longer has a person lookup.
    var person = await _tmdbService.FindPersonByImdbIdAsync(request.ImdbId, cancellationToken);

    if (person is null)
    {
      return Result.Failure<GetPersonFilmographyResponse>(MovieErrors.NotFound(request.ImdbId));
    }

    var credits = await _tmdbService.GetPersonCombinedCreditsAsync(
      person.TmdbId,
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
        PersonName = person.Name,
        PersonPhotoUrl = person.PhotoUrl,
        Credits = mapped,
      }
    );
  }
}
