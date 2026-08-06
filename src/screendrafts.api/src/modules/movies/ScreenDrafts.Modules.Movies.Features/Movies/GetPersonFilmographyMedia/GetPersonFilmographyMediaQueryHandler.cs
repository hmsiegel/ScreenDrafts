using System.Net.Http.Headers;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed class GetPersonFilmographyMediaQueryHandler(
  IIntegrationsApi integrationsApi,
  IMediaRepository mediaRepository
) : IQueryHandler<GetPersonFilmographyMediaQuery, GetPersonFilmographyMediaResponse>
{
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly IMediaRepository _mediaRepository = mediaRepository;

  public async Task<Result<GetPersonFilmographyMediaResponse>> Handle(
    GetPersonFilmographyMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    var filmographyResult = await _integrationsApi.GetPersonFilmographyAsync(
      request.ImdbId,
      cancellationToken
    );

    if (filmographyResult.IsFailure)
    {
      return Result.Failure<GetPersonFilmographyMediaResponse>(filmographyResult.Errors);
    }

    var credits = filmographyResult.Value.Credits;
    var tmdbIds = credits.Select(c => c.TmdbId).Distinct().ToList();
    var tmdbIdMediaTypePairs = credits
      .Select(c => (c.TmdbId, MediaTypeHeaderValue: c.MediaType))
      .Distinct()
      .ToList();

    var existingTmdbIds =
      tmdbIds.Count > 0
        ? await _mediaRepository.GetExistingMediaTmdbsAsync(tmdbIds, cancellationToken)
        : [];

    var publicIdsByTmdbId =
      tmdbIdMediaTypePairs.Count > 0
        ? await _mediaRepository.GetPublicIdsByTmdbIdsAsync(tmdbIdMediaTypePairs, cancellationToken)
        : [];

    var mappedCredits = credits
      .Select(c =>
      {
        var isInDatabase = existingTmdbIds.Contains((c.TmdbId, c.MediaType));

        return new FilmographyMediaCreditResponse
        {
          TmdbId = c.TmdbId,
          Title = c.Title,
          Year = c.Year,
          PosterUrl = c.PosterPath,
          MediaType = c.MediaType,
          CreditRole = c.CreditRole,
          IsInMediaDatabase = isInDatabase,
          MediaPublicId = isInDatabase
            ? publicIdsByTmdbId.GetValueOrDefault((c.TmdbId, c.MediaType))
            : null,
        };
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new GetPersonFilmographyMediaResponse
      {
        PersonName = filmographyResult.Value.PersonName,
        PersonPhotoUrl = filmographyResult.Value.PersonPhotoPath,
        Credits = mappedCredits,
      }
    );
  }
}
