namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record SearchPeopleQuery : IQuery<PagedResult<SearchPeopleResponse>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? Name { get; init; }
  public string? Role { get; init; }
}


