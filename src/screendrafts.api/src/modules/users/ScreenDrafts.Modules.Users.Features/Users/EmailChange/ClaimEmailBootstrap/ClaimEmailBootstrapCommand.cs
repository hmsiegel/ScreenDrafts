namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ClaimEmailBootstrap;

internal sealed record ClaimEmailBootstrapCommand : ICommand
{
  public string Token { get; init; } = default!;
  public string NewEmail { get; init; } = default!;
}
