namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;

internal sealed record EmailBootstrapTokenResponse
{
  public string UserPublicId { get; init; } = default!;
  public string CurrentEmail { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public bool IsPatreon { get; init; }
  public string Token { get; init; } = default!;
  public DateTimeOffset ExpiresAt { get; init; }
}
