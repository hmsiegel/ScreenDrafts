namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonProfile;

internal sealed class GetPersonProfileCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<GetPersonProfileCommand, GetPersonProfileResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<GetPersonProfileResponse>> Handle(
    GetPersonProfileCommand request,
    CancellationToken cancellationToken
  )
  {
    TmdbPersonProfile? person = null;

    if (request.TmdbId > 0)
    {
      person = await _tmdbService.GetPersonProfileAsync(request.TmdbId, cancellationToken);
    }
    else if (!string.IsNullOrWhiteSpace(request.ImdbId))
    {
      person = await _tmdbService.FindPersonByImdbIdAsync(request.ImdbId, cancellationToken);
    }

    if (person is null)
    {
      return request.TmdbId > 0
        ? Result.Failure<GetPersonProfileResponse>(MovieErrors.NotFound(request.TmdbId))
        : Result.Failure<GetPersonProfileResponse>(
          MovieErrors.NotFound(request.ImdbId ?? string.Empty)
        );
    }

    return Result.Success(
      new GetPersonProfileResponse
      {
        TmdbId = person.TmdbId,
        Name = person.Name,
        PhotoUrl = person.PhotoUrl,
      }
    );
  }
}
