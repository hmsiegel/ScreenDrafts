namespace ScreenDrafts.Modules.Integrations.Features.Movies;

internal sealed class FetchMediaIntegrationEventHandler(
  ISender sender,
  ILogger<FetchMediaIntegrationEventHandler> logger)
    : IntegrationEventHandler<FetchMediaRequestedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<FetchMediaIntegrationEventHandler> _logger = logger;

  public override async Task Handle(
    FetchMediaRequestedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var command = new FetchMediaCommand
    {
      MediaType = integrationEvent.MediaType,
      ImdbId = integrationEvent.ImdbId,
      TmdbId = integrationEvent.TmdbId,
      IgdbId = integrationEvent.IgdbId,
      TvSeriesTmdbId = integrationEvent.TvSeriesTmdbId,
      SeasonNumber = integrationEvent.SeasonNumber,
      EpisodeNumber = integrationEvent.EpisodeNumber
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      var id = integrationEvent.TmdbId?.ToString(CultureInfo.InvariantCulture)
        ?? integrationEvent.IgdbId?.ToString(CultureInfo.InvariantCulture)
        ?? "Unknown";
      MovieLoggingMessages.MovieAlreadyExists(_logger, id);
    }
  }
}
