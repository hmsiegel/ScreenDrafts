namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository poolRepository,
  IDraftBoardRepository boardRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<VetoOverrideAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _poolRepository = poolRepository;
  private readonly IDraftBoardRepository _boardRepository = boardRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(VetoOverrideAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(draftPartPublicId: domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    var draftPart = await _draftPartRepository.GetByIdAsync(
      draftPartId: DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken: cancellationToken);

    if (draftPart is not null)
    {
      var pool = await _poolRepository.GetByDraftIdAsync(draftPart.DraftId, cancellationToken);

      if (pool is not null)
      {
        pool.RemoveMovie(domainEvent.TmdbId);

        _poolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          key: DraftsCacheKeys.DraftPool(domainEvent.DraftPublicId),
          cancellationToken: cancellationToken);
      }
      else
      {
        var participant = await _participantResolver.ResolveByParticpantIdAsync(
          participantId: domainEvent.ParticipantId,
          participantKind: ParticipantKind.FromValue(domainEvent.ParticipantKind),
          cancellationToken: cancellationToken);

        var board = await _boardRepository.GetByDraftAndParticipantAsync(
          draftId: draftPart.DraftId,
          participantId: participant.Value,
          cancellationToken: cancellationToken);

        if (board is not null)
        {
          board.RemoveItem(domainEvent.TmdbId);

          _boardRepository.Update(board);

          await _cacheService.RemoveAsync(
            key: DraftsCacheKeys.DraftBoard(domainEvent.DraftPublicId, domainEvent.ParticipantId),
            cancellationToken: cancellationToken);
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    await _eventBus.PublishAsync(new VetoOverrideAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId),
      cancellationToken);

    if (domainEvent.CanonicalPolicyValue == 1)
    {
      return;
    }

    var hasMainFeedRelease = draftPart?.Releases
      .Any(r => r.ReleaseChannel == ReleaseChannel.MainFeed) ?? false;

    if (domainEvent.CanonicalPolicyValue == 2 && !hasMainFeedRelease)
    {
      return;
    }

    var pick = draftPart!.Picks.FirstOrDefault(p =>
      p.Movie.TmdbId == domainEvent.TmdbId && p.IsActiveOnFinalBoard);

    if (pick is null)
    {
      return;
    }

    await _eventBus.PublishAsync(
      new PickLockedIntegrationEvent(
        Guid.NewGuid(),
        _dateTimeProvider.UtcNow,
        domainEvent.DraftPartId,
        domainEvent.DraftPartPublicId,
        domainEvent.DraftId,
        domainEvent.DraftPublicId,
        pick.Movie.PublicId,
        pick.Movie.MovieTitle,
        pick.Movie.TmdbId,
        pick.Position,
        domainEvent.ParticipantId,
        domainEvent.ParticipantKind,
        domainEvent.CanonicalPolicyValue,
        hasMainFeedRelease),
      cancellationToken);
  }
}
