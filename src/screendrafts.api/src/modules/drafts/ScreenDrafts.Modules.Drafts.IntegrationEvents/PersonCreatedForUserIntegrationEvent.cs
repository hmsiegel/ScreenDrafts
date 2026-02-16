using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PersonCreatedForUserIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid personId,
  string personPublicId,
  Guid userId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid UserId { get; init; } = userId;
  public Guid PersonId { get; init; } = personId;
  public string PersonPublicId { get; init; } = personPublicId;
}
