namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ListCandidates;

internal sealed record EmailBootstrapCandidateItem
{
  public string UserPublicId { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string CurrentEmail { get; init; } = default!;
  public bool IsPatreon { get; init; }
  public bool HasActiveToken { get; init; }
  public DateTimeOffset? TokenExpiresAt { get; init; }
};
