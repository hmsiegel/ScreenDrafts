namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ListCandidates;

internal sealed record ListEmailBootstrapCandidatesQuery
  : IQuery<PagedResult<EmailBootstrapCandidateItem>>
{
  public string? Search { get; init; }
  public int Page { get; init; }
  public int PageSize { get; init; }
};
