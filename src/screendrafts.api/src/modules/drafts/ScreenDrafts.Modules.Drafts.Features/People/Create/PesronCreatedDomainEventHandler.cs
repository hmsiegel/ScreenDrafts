using IEventBus = ScreenDrafts.Common.Features.Abstractions.EventBus.IEventBus;

namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class PesronCreatedDomainEventHandler(
  ISender sender,
  IEventBus eventBus)
  : DomainEventHandler<PersonCreatedDomainEvent>
{
  private readonly ISender _sender = sender;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(PersonCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    var query = new Features.People.Get.Query(domainEvent.PublicId);

    var result = await _sender.Send(query, cancellationToken);

    await _eventBus.PublishAsync(
      new PersonCreatedForUserIntegrationEvent(
        domainEvent.Id,
        domainEvent.OccurredOnUtc,
        domainEvent.PersonId,
        result.Value.PublicId,
        result.Value.UserId),
      cancellationToken: cancellationToken);
  }
}
