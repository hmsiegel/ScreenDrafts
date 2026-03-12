namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository draftPoolRepository,
  IDraftBoardRepository draftBoardRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork)
  : DomainEventHandler<VetoAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(VetoAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(draftPartPublicId: domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken);

    if (draftPart is not null)
    {
      var pool = await _draftPoolRepository.GetByDraftIdAsync(
        draftPart.DraftId,
        cancellationToken);

      if (pool is not null)
      {
        pool.RestoreMovie(domainEvent.TmdbId!.Value);

        _draftPoolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          key: DraftsCacheKeys.DraftPool(draftPublicId: domainEvent.DraftPublicId),
          cancellationToken: cancellationToken);
      }
      else
      {
        var participant = await _participantResolver.ResolveByParticpantIdAsync(
          participantId: domainEvent.ParticipantId,
          participantKind: ParticipantKind.FromValue(domainEvent.ParticipantKind),
          cancellationToken: cancellationToken);

        var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
          draftPart.DraftId,
          participant.Value,
          cancellationToken);

        if (board is not null)
        {
          board.AddItem(domainEvent.TmdbId!.Value, notes: null, priority: null);

          _draftBoardRepository.Update(board);

          await _cacheService.RemoveAsync(
            key: DraftsCacheKeys.DraftBoard(draftPublicId: domainEvent.DraftPublicId, userId: domainEvent.ParticipantId),
            cancellationToken: cancellationToken);
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    await _eventBus.PublishAsync(new VetoAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId),
      cancellationToken);
  }
}
