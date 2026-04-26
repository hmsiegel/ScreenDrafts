namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed class PickSubmittedDomainEventHandler(
  IPickRepository pickRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<PickAddedDomainEvent>
{
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    PickAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    var pickId = PickId.Create(domainEvent.PickId);

    var pick = await _pickRepository.GetByIdAsync(
      pickId,
      cancellationToken);

    if (pick is null)
    {
      return;
    }

    if (pick.IsRevealed)
    {
      return;
    }

    await _eventBus.PublishAsync(new PickSubmittedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: _dateTimeProvider.UtcNow,
      draftPartId: domainEvent.DraftPartId,
      draftPartPublicId: domainEvent.DraftPartPublicId,
      playOrder: domainEvent.PlayOrder,
      movieId: pick.MovieId,
      moviePublicId: domainEvent.MoviePublicId,
      movieTitle: domainEvent.MovieTitle,
      imdbId: domainEvent.ImdbId,
      tmdbId: domainEvent.TmdbId,
      boardPosition: domainEvent.BoardPosition,
      participantId: domainEvent.ParticipantId,
      participantKind: domainEvent.ParticipantKind,
      actedByPublicId: pick.ActedByPublicId),
      cancellationToken);
  }
}
