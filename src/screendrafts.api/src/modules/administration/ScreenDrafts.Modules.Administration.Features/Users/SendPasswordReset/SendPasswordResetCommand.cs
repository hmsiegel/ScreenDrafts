namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed record SendPasswordResetCommand : ICommand
{
  public string PublicId { get; init; } = default!;
}
