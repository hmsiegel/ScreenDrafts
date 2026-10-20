namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class SearchImdbPeopleCommandHandler(IImdbService imdbService)
  : ICommandHandler<SearchImdbPeopleCommand, SearchImdbPeopleResponse>
{
  private readonly IImdbService _imdbService = imdbService;

  public async Task<Result<SearchImdbPeopleResponse>> Handle(
    SearchImdbPeopleCommand request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchImdbPeopleResponse>(MovieErrors.SearchQueryRequired);
    }

    var searchData = await _imdbService.SearchByName(request.Query);

    // The IMDb API reports a bad key or exhausted quota as an HTTP 200 with
    // ErrorMessage set and no results. Return it instead of an empty list.
    if (searchData is not null && !string.IsNullOrEmpty(searchData.ErrorMessage))
    {
      return Result.Failure<SearchImdbPeopleResponse>(
        MovieErrors.ImdbSearchFailed(searchData.ErrorMessage)
      );
    }

    var mapped = (searchData?.Items ?? [])
      .Select(r => new ImdbPersonSearchResult
      {
        ImdbId = r.IMDbId,
        Name = r.Name,
        Description = r.Professions,
        PhotoUrl = r.Image,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchImdbPeopleResponse { Results = mapped });
  }
}
