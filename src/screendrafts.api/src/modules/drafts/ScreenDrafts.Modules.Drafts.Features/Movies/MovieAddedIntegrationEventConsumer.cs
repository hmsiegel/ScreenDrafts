using ScreenDrafts.Modules.Drafts.Features.Movies.Add;

namespace ScreenDrafts.Modules.Drafts.Features.Movies;

internal sealed class MovieAddedIntegrationEventConsumer(
  ISender sender,
  ILogger<MovieAddedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<MovieAddedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<MovieAddedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MovieAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new AddMovieCommand
      {
        ImdbId = integrationEvent.ImdbId,
        Id = integrationEvent.MovieId,
        Title = integrationEvent.Title
      },
      cancellationToken);

    if (result.IsFailure)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, integrationEvent.ImdbId);
    }
  }
}
