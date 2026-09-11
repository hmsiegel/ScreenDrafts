using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Movies;

internal sealed partial class MediaAddedIntegrationEventConsumer(
  ISender sender,
  ILogger<MediaAddedIntegrationEventConsumer> logger
) : IntegrationEventHandler<MediaAddedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<MediaAddedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MediaAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new AddMovieCommand
      {
        Id = integrationEvent.MediaId,
        PublicId = integrationEvent.PublicId,
        Title = integrationEvent.Title,
        ImdbId = integrationEvent.ImdbId,
        TmdbId = integrationEvent.TmdbId,
        IgdbId = integrationEvent.IgdbId,
        MediaType = integrationEvent.MediaType,
        Year = integrationEvent.Year,
        TvSeriesTmdbId = integrationEvent.TvSeriesTmdbId,
        SeasonNumber = integrationEvent.SeasonNumber,
        EpisodeNumber = integrationEvent.EpisodeNumber,
        TvSeriesTitle = integrationEvent.TvSeriesTitle,
      },
      cancellationToken
    );

    if (result.IsFailure)
    {
      LogAddFailed(_logger, integrationEvent.PublicId, result.Errors);
    }
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Failed to add Movies {PublicId} with errors {Errors}"
  )]
  private static partial void LogAddFailed(ILogger logger, string publicId, object errors);
}
