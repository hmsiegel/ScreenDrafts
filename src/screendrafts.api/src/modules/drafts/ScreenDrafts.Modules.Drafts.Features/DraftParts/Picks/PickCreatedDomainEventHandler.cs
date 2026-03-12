namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickCreatedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository draftPoolRepository,
  IDraftBoardRepository draftBoardRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork)
  : DomainEventHandler<PickAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(PickAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId), cancellationToken);

    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken);

    if (draftPart is not null)
    {
      var pool = await _draftPoolRepository.GetByDraftIdAsync(draftPart.DraftId, cancellationToken);

      if (pool is not null)
      {
        pool.RemoveMovie(domainEvent.TmdbId!.Value);

        _draftPoolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          DraftsCacheKeys.DraftPool(domainEvent.DraftPublicId),
          cancellationToken);
      }
      else
      {
        var participant = await _participantResolver.ResolveByParticpantIdAsync(
          domainEvent.ParticipantId,
          ParticipantKind.FromValue(domainEvent.ParticipantKind),
          cancellationToken);

        var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
          draftPart.DraftId,
          participant.Value,
          cancellationToken);

        if (board is not null)
        {
          board.RemoveItem(domainEvent.TmdbId!.Value);
          _draftBoardRepository.Update(board);

          await _cacheService.RemoveAsync(
            DraftsCacheKeys.DraftBoard(domainEvent.DraftPublicId, domainEvent.ParticipantId),
            cancellationToken);
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    await _eventBus.PublishAsync(new PickAddedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId,
      domainEvent.ImdbId,
      domainEvent.MovieTitle),
      cancellationToken);
  }
}
