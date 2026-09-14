using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Users.IntegrationEvents;

public sealed class EmailChangeConfirmationRequestedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid userId,
  string newEmail,
  string confirmationLink
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; init; } = userId;

  public string NewEmail { get; init; } = newEmail;

  public string ConfirmationLink { get; init; } = confirmationLink;
}
