namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Confirm;

internal sealed record ConfirmEmailChangeRequest
{
  public required string Token { get; init; }
}
