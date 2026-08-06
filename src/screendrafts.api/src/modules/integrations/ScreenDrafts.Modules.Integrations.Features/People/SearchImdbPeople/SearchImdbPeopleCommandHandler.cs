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

    var mapped = (searchData?.Results ?? [])
      .Select(r => new ImdbPersonSearchResult
      {
        ImdbId = r.Id,
        Name = r.Title,
        Description = r.Description,
        PhotoUrl = r.Image,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchImdbPeopleResponse { Results = mapped });
  }
}
