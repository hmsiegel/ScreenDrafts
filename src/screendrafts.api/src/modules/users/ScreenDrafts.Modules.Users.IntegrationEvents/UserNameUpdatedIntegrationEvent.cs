using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class UserNameUpdatedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string firstName,
  string lastName,
  string? middleName
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; init; } = userId;
  public string FirstName { get; init; } = firstName;
  public string LastName { get; init; } = lastName;
  public string? MiddleName { get; init; } = middleName;
}
