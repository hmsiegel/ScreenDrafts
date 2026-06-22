namespace ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;

internal sealed class UpdateMovieHonorificCommandHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<UpdateMovieHonorificCommand>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    UpdateMovieHonorificCommand request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var currentTime = _dateTimeProvider.UtcNow;

    const string insertPick = """
      INSERT INTO reporting.movie_canonical_picks
        (id, movie_public_id, draft_part_public_id, board_position, picked_at)
      VALUES
        (@Id, @MoviePublicId, @DraftPartPublicId, @BoardPosition, @PickedAt)
      ON CONFLICT (movie_public_id, draft_part_public_id) DO NOTHING;
      """;

    var inserted = await connection.ExecuteAsync(
      new CommandDefinition(
        insertPick,
        new
        {
          Id = Guid.NewGuid(),
          request.MoviePublicId,
          request.DraftPartPublicId,
          request.BoardPosition,
          PickedAt = currentTime,
        },
        cancellationToken: cancellationToken
      )
    );

    if (inserted == 0)
    {
      // The row for this (movie, draft part) pair already exists, so nothing about the movie's
      // canonical appearance set has changed — most likely PickLockedIntegrationEvent was
      // delivered more than once for the same pick (MassTransit retries / at-least-once
      // delivery) rather than a genuine new lock. A re-lock that follows a real revert is NOT
      // this branch: RevertMovieHonorificCommandHandler deletes the row on unlock, so a
      // subsequent lock (undo-veto, veto-override) finds no row and inserted == 1 below,
      // correctly triggering a fresh recompute.
      return Result.Success();
    }

    await MovieHonorificRecompute.RunAsync(
      connection: connection,
      moviePublicId: request.MoviePublicId,
      movieTitle: request.MovieTitle,
      draftPartPublicId: request.DraftPartPublicId,
      currentTime: currentTime,
      eventBus: _eventBus,
      cancellationToken: cancellationToken
    );

    return Result.Success();
  }
}
