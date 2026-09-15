namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;

internal sealed record ValidateEmailBootstrapTokenQuery
  : IQuery<EmailBootstrapTokenValidationResponse>
{
  public string Token { get; init; } = default!;
}
