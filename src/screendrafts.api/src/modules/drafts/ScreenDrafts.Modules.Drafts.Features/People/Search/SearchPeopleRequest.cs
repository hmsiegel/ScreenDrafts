namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record SearchPeopleRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 10;

  [FromQuery(Name = "name")]
  public string? Name { get; init; }

  [FromQuery(Name = "role")]
  public string? Role { get; init; }
}

