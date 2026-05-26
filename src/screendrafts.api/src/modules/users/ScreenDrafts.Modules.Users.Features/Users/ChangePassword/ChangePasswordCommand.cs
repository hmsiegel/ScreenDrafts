namespace ScreenDrafts.Modules.Users.Features.Users.ChangePassword;

internal sealed record ChangePasswordCommand : ICommand
{
  public required string PublicId { get; init; }
  public required string CurrentPassword { get; init; }
  public required string Password { get; init; }
}
