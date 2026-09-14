using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class UserEmailChangedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string newEmail
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; init; } = userId;

  public string NewEmail { get; init; } = newEmail;
}
