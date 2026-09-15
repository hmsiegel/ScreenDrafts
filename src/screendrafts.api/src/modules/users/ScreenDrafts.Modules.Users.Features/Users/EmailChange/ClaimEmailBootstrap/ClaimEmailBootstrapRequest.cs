namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ClaimEmailBootstrap;

internal sealed record ClaimEmailBootstrapRequest
{
  public string Token { get; init; } = default!;
  public string NewEmail { get; init; } = default!;
}
