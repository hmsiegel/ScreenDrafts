namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ListCandidates;

internal sealed record ListEmailBootstrapCandidatesRequest
{
  [FromQuery(Name = "search")]
  public string? Search { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 25;
}
