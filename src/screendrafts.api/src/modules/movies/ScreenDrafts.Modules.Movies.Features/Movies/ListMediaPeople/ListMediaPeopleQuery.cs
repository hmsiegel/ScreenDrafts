namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal sealed record ListMediaPeopleQuery : IQuery<ListMediaPeopleResponse>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 48;
  public string? Search { get; init; }
  public string? Role { get; init; }
  public string? Sort { get; init; }
}
