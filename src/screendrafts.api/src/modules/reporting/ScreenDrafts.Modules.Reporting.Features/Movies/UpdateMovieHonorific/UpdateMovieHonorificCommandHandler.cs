namespace ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;

internal sealed class UpdateMovieHonorificCommandHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<UpdateMovieHonorificCommand>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(UpdateMovieHonorificCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var currentTime = _dateTimeProvider.UtcNow;

    const string insertPick =
      """
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
          PickedAt = currentTime
        },
        cancellationToken: cancellationToken));

    if (inserted == 0)
    {
      return Result.Success();
    }

    const string picksSql =
      """
      SELECT mcp.board_position
      FROM reporting.movie_canonical_picks mcp
      WHERE mcp.movie_public_id = @MoviePublicId;
      """;

    var positions = (await connection.QueryAsync<int>(
      new CommandDefinition(
        picksSql,
        new { request.MoviePublicId },
        cancellationToken: cancellationToken)))
      .ToList();

    var appearanceCount = positions.Count;

    const string currentSql =
      """
      SELECT appearance_honorific, position_honorific
      FROM reporting.movie_honorifics
      WHERE movie_public_id = @MoviePublicId;
      """;

    var current = await connection.QuerySingleOrDefaultAsync<(int AppearanceHonorific, int PositionHonorific)>(
      new CommandDefinition(
        currentSql,
        new { request.MoviePublicId },
        cancellationToken: cancellationToken));

    var previousAppearanceHonorific = MovieHonorific.FromValue(current.AppearanceHonorific);
    var previousPositionHonorific = (MoviePositionHonorific)(current.PositionHonorific);

    var newAppearanceHonorific = MovieHonorific.FromAppearanceCount(appearanceCount);
    var newPositionHonorific = MoviePositionHonorific.None;

    if (positions.Count(p => p == 1) >= 2)
    {
      newPositionHonorific |= MoviePositionHonorific.UnifiedNumber1;
    }

    var distinctPositions = positions.ToHashSet();
    if (distinctPositions.Contains(1) &&
      distinctPositions.Contains(2) &&
      distinctPositions.Contains(3) &&
      distinctPositions.Contains(4))
    {
      newPositionHonorific |= MoviePositionHonorific.TheCycle;
    }

    const string upsertSql =
      """
      INSERT INTO reporting.movie_honorifics
        (id, movie_public_id, movie_title, appearance_honorific, position_honorific, appearance_count, update_at_utc)
      VALUES
        (@Id, @MoviePublicId, @MovieTitle, @AppearanceHonorificValue, @PositionHonorific, @AppearanceCount, @UpdatedAt)
      ON CONFLICT (movie_public_id) DO UPDATE
        SET
          movie_title = EXCLUDED.movie_title,
          appearance_honorific = EXCLUDED.appearance_honorific,
          position_honorific = EXCLUDED.position_honorific,
          appearance_count = EXCLUDED.appearance_count,
          update_at_utc = EXCLUDED.update_at_utc
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        upsertSql,
        new
        {
          Id = Guid.NewGuid(),
          request.MoviePublicId,
          request.MovieTitle,
          AppearanceHonorificValue = newAppearanceHonorific.Value,
          PositionHonorific = (int)newPositionHonorific,
          AppearanceCount = appearanceCount,
          UpdatedAt = currentTime
        },
        cancellationToken: cancellationToken));

    var appearanceChanged = newAppearanceHonorific != previousAppearanceHonorific;
    var positionChanged = newPositionHonorific != previousPositionHonorific;

    if (!appearanceChanged && !positionChanged)
    {
      return Result.Success();
    }

    const string historySql =
      """
      INSERT INTO reporting.movies_honorifics_history
       (id, movie_public_id, appearance_honorific, position_honorific, appearance_count, achieved_at)
      VALUES
       (@Id, @MoviePublicId, @AppearanceHonorificValue, @PositionHonorific, @AppearanceCount, @AchievedAt);
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        historySql,
        new
        {
          Id = Guid.NewGuid(),
          request.MoviePublicId,
          AppearanceHonorificValue = newAppearanceHonorific.Value,
          PositionHonorific = (int)newPositionHonorific,
          AppearanceCount = appearanceCount,
          AchievedAt = currentTime
        },
        cancellationToken: cancellationToken));

    await _eventBus.PublishAsync(
      new MovieHonorificEarnedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: currentTime,
        moviePublicId: request.MoviePublicId,
        movieTitle: request.MovieTitle,
        draftPartPublicId: request.DraftPartPublicId,
        previousAppearanceHonorificValue: previousAppearanceHonorific.Value,
        newAppearanceHonorificValue: newAppearanceHonorific.Value,
        previousPositionHonorificValue: (int)previousPositionHonorific,
        newPositionHonorificValue: (int)newPositionHonorific,
        appearanceCount: appearanceCount),
      cancellationToken);

    return Result.Success();

  }
}
