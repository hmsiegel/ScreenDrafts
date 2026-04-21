using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class PermissionAddedToRoleIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string roleName,
  string permissionCode,
  IReadOnlyList<Guid> affectedUserIds)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string RoleName { get; set; } = roleName;
  public string PermissionCode { get; set; } = permissionCode;
  public IReadOnlyList<Guid> AffectedUserIds { get; set; } = affectedUserIds;
}
