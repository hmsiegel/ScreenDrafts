namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;

internal sealed record EmailBootstrapTokenValidationResponse
{
  public bool IsValid { get; init; }
  public string UserPublicId { get; init; } = default!;
}
