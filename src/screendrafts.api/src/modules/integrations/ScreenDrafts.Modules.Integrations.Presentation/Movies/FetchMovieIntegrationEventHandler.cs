namespace ScreenDrafts.Modules.Integrations.Presentation.Movies;

using ScreenDrafts.Common.Features.Abstractions.EventBus;

internal sealed class FetchMovieIntegrationEventHandler(
  ISender sender,
  ILogger<FetchMovieIntegrationEventHandler> logger)
    : IntegrationEventHandler<FetchMovieRequestedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<FetchMovieIntegrationEventHandler> _logger = logger;

  public override async Task Handle(
    FetchMovieRequestedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new FetchMovieCommand(integrationEvent.ImdbId),
      cancellationToken);

    if (result.IsFailure)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, integrationEvent.ImdbId);
    }
  }
}
