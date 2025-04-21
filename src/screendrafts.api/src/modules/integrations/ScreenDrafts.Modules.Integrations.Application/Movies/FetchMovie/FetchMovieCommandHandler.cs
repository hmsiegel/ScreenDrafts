namespace ScreenDrafts.Modules.Integrations.Application.Movies.FetchMovie;

internal sealed class FetchMovieCommandHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  ISender sender)
  : ICommandHandler<FetchMovieCommand>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ISender _sender = sender;

  public async Task<Result> Handle(FetchMovieCommand command, CancellationToken cancellationToken)
  {
    var response = await _sender.Send(new GetOnlineMovieCommand(command.ImdbId), cancellationToken);

    if (response.IsFailure)
    {
      return Result.Failure(MovieErrors.NotFound(command.ImdbId));
    }

    await _eventBus.PublishAsync(new MovieFetchedIntegrationEvent(
      Guid.NewGuid(),
      _dateTimeProvider.UtcNow,
      command.ImdbId,
      response.Value.Title!,
      response.Value.Year,
      response.Value.Plot,
      response.Value.Image,
      response.Value.ReleaseDate,
      response.Value.YouTubeTrailerUri,
      response.Value.Genres,
      response.Value.Actors,
      response.Value.Directors,
      response.Value.Writers,
      response.Value.Producers,
      response.Value.ProductionCompanies),
      cancellationToken);

    return Result.Success();
  }
}
