using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class UserRoleAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string roleName,
  IReadOnlyList<string> permissionCodesToAdd)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; set; } = userId;
  public string RoleName { get; set; } = roleName;
  public IReadOnlyList<string> PermissionCodesToAdd { get; set; } = permissionCodesToAdd;
}
