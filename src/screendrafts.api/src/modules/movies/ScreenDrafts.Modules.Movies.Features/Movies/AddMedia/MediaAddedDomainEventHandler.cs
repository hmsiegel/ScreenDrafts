using ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class MediaAddedDomainEventHandler(ISender sender, IEventBus eventBus)
  : DomainEventHandler<MediaCreatedDomainEvent>
{
  private readonly ISender _sender = sender;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    MediaCreatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    var query = new GetMediaQuery 
    {
      PublicId = domainEvent.PublicId
    };

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(GetMediaQuery), result.Error);
    }

    var r = result.Value;

    await _eventBus.PublishAsync(
      new MediaAddedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        mediaId: r.Id,
        title: r.Title,
        imdbId: r.ImdbId!,
        tmdbId: r.TmdbId!.Value,
        publicId: r.PublicId,
        mediaType: r.MediaType,
        igdbId: r.IgdbId),
      cancellationToken);
  }
}
