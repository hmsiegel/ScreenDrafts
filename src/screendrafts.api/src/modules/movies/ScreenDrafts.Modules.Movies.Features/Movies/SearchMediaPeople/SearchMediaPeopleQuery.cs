namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed record SearchMediaPeopleQuery : IQuery<SearchMediaPeopleResponse>
{
  public string? Query { get; init; }
  public string? Role { get; init; }
  public int Limit { get; init; } = 10;
}
