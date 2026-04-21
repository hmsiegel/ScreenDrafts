using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class PermissionRemovedFromRoleIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string roleName,
  string permissionCode,
  IReadOnlySet<Guid> affectedUserIds)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string RoleName { get; set; } = roleName;
  public string PermissionCode { get; set; } = permissionCode;
  public IReadOnlySet<Guid> AffectedUserIds { get; set; } = affectedUserIds;
}
