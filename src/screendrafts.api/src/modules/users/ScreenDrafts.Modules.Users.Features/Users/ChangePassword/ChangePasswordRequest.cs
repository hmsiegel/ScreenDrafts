namespace ScreenDrafts.Modules.Users.Features.Users.ChangePassword;

internal sealed record ChangePasswordRequest
{
  public required string CurrentPassword { get; init; }
  public required string Password { get; init; }
  public required string ConfirmPassword { get; init; }
}
