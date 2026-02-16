namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record SearchPeopleRequest
{
  [FromQuery(Name = "q")]
  public string Search { get; init; } = default!;

  [FromQuery(Name = "limit")]
  public int Limit { get; init; } = 20;
}

