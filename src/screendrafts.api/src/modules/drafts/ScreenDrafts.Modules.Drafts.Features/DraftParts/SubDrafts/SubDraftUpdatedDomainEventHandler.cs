namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts;

internal sealed class SubDraftUpdatedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<SubDraftUpdatedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    SubDraftUpdatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new SubDraftUpdatedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        subDraftPublicId: domainEvent.SubDraftPublicId,
        status: domainEvent.Status,
        subjectKind: domainEvent.SubjectKind,
        subjectName: domainEvent.SubjectName,
        subjectImdbId: domainEvent.SubjectImdbId
      ),
      cancellationToken
    );
  }
}
