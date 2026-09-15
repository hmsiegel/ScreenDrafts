namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Confirm;

internal sealed record ConfirmEmailChangeCommand : ICommand
{
  public required string Token { get; init; }
}
