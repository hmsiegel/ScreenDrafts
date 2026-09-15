namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;

internal sealed record ValidateEmailBootstrapTokenRequest
{
  public string Token { get; init; } = default!;
}
