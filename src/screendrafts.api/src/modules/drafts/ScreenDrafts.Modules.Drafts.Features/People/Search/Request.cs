namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record Request
{
  [Microsoft.AspNetCore.Mvc.FromQuery(Name = "q")]
  public string Search { get; init; } = default!;

  [Microsoft.AspNetCore.Mvc.FromQuery(Name = "limit")]
  public int Limit { get; init; } = 20;
}
