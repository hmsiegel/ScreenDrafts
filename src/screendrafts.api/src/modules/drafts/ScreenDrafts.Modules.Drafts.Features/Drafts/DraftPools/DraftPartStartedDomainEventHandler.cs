namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools;

internal sealed class DraftPartStartedDomainEventHandler(
  IDraftPoolRepository draftPoolRepository,
  IUnitOfWork unitOfWork,
  IDraftPartRepository draftPartRepository,
  IEventBus eventBus,
  ISeriesPolicyProvider seriesPolicyProvider,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<DraftPartStartedDomainEvent>
{
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(DraftPartStartedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    var pool = await _draftPoolRepository.GetByDraftIdAsync(
      DraftId.Create(domainEvent.DraftId),
      cancellationToken);

    if (pool is not null)
    {
      pool.Lock();
      _draftPoolRepository.Update(pool);

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken);

    if (draftPart is null)
    {
      return;
    }

    var series = await _seriesPolicyProvider.GetSeriesAsyc(draftPart.SeriesId, cancellationToken);

    if (series is null)
    {
      return;
    }

    /// CanonicalPolicy.Never nothing to report
    if (series.CanonicalPolicy == CanonicalPolicy.Never)
    {
      return;
    }

    var hasMainFeedRelease = draftPart.Releases
      .Any(r => r.ReleaseChannel == ReleaseChannel.MainFeed);

    if (series.CanonicalPolicy == CanonicalPolicy.OnMainFeed && !hasMainFeedRelease)
    {
      return;
    }

    var participants = draftPart.Participants
      .Where(p => p.Kind == ParticipantKind.Drafter)
      .Select(p => new DraftPartParticipantModel
      {
        ParticipantIdValue = p.Value,
        ParticipantKindValue = p.Kind.Value
      })
      .ToList();

    await _eventBus.PublishAsync(
      new DraftPartStartedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: draftPart.PublicId,
        draftId: domainEvent.DraftId,
        draftPublicId: domainEvent.DraftPublicId,
        partIndex: domainEvent.Index,
        participants: participants,
        canonicalPolicyValue: series.CanonicalPolicy.Value,
        hasMainFeedRelease: hasMainFeedRelease),
      cancellationToken);
  }
}
