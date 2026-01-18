using ScreenDrafts.Common.Features.Abstractions.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string email,
  string firstName,
  string lastName,
  string? middleName)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; init; } = userId;

  public string Email { get; init; } = email;

  public string FirstName { get; init; } = firstName;

  public string LastName { get; init; } = lastName;

  public string? MiddleName { get; init; } = middleName;
}
