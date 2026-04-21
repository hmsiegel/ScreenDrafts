using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class UserRoleRemovedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string roleName,
  IReadOnlyList<string> permissionCodesToRemove)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; set; } = userId;
  public string RoleName { get; set; } = roleName;
  public IReadOnlyList<string> PermissionCodesToRemove { get; set; } = permissionCodesToRemove;
}
