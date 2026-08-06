namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed class SearchGamesCommandHandler(IIgdbService igdbService)
  : ICommandHandler<SearchGamesCommand, SearchGamesResponse>
{
  private readonly IIgdbService _igdbService = igdbService;

  public async Task<Result<SearchGamesResponse>> Handle(
    SearchGamesCommand request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchGamesResponse>(MovieErrors.SearchQueryRequired);
    }

    var results = await _igdbService.SearchGamesAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    var mapped = results
      .Select(g => new GameSearchResult
      {
        IgdbId = g.Id,
        Title = g.Name,
        Year = g.FirstReleaseDate.HasValue
          ? DateTimeOffset
            .FromUnixTimeSeconds(g.FirstReleaseDate.Value)
            .Year.ToString(CultureInfo.InvariantCulture)
          : null,
        PosterUrl = g.CoverUrl?.ToString(),
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchGamesResponse { Results = mapped });
  }
}
