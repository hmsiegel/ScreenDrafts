namespace ScreenDrafts.Modules.Drafts.Features.Movies;

internal sealed class MediaAddedIntegrationEventConsumer(
  ISender sender,
  ILogger<MediaAddedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<MediaAddedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<MediaAddedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MediaAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new AddMovieCommand
      {
        Id = integrationEvent.Id,
        PublicId = integrationEvent.PublicId,
        Title = integrationEvent.Title,
        ImdbId = integrationEvent.ImdbId,
        TmdbId = integrationEvent.TmdbId,
        IgdbId = integrationEvent.IgdbId,
        MediaType = integrationEvent.MediaType
      },
      cancellationToken);

    if (result.IsFailure)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, integrationEvent.PublicId);
    }
  }
}
