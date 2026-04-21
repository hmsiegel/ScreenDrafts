namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed record AddPermissionCommand : ICommand
{
  public required string Code { get; init; }
}
