namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class PickRevealedDomianEventHandler(
  IPickRepository pickRepository,
  IEventBus eventBus,
  ICacheService cacheService,
  IDateTimeProvider dateTimeProvider,
  IMovieRepository movieRepository)
  : DomainEventHandler<PickRevealedDomainEvent>
{
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IMovieRepository _movieRepository = movieRepository;

  public override async Task Handle(
    PickRevealedDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    var pickId = PickId.Create(domainEvent.PickId);

    var pick = await _pickRepository.GetByIdAsync(
      id: pickId,
      cancellationToken: cancellationToken);

    if (pick is null)
    {
      return;
    }

    var movie = await _movieRepository.GetByIdAsync(domainEvent.MovieId, cancellationToken: cancellationToken);

    if (movie is null)
    {
      return;
    }

    await _eventBus.PublishAsync(new PickRevealedIntegrationEvent(
      Guid.NewGuid(),
      _dateTimeProvider.UtcNow,
      domainEvent.DraftPartId,
      domainEvent.DraftPartPublicId,
      domainEvent.PlayOrder,
      domainEvent.MovieId,
      movie.PublicId,
      movie.MovieTitle,
      movie.ImdbId,
      movie.TmdbId,
      pick.Position,
      pick.PlayedByParticipantIdValue,
      pick.PlayedByParticipantKindValue.Value,
      domainEvent.ActedByPublicId),
      cancellationToken);
  }
}
