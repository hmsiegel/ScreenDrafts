using System.Data;

namespace ScreenDrafts.Modules.Reporting.Features.Movies;

/// <summary>
/// Shared recompute step for movie honorifics. Both <see cref="UpdateMovieHonorificCommandHandler"/>
/// (fired when a pick locks) and <see cref="RevertMovieHonorificCommandHandler"/> (fired when a
/// previously-locked pick is vetoed or commissioner-overridden) need to do the exact same thing
/// after their respective INSERT or DELETE against reporting.movie_canonical_picks: re-read every
/// remaining canonical pick row for the movie, recompute the appearance count and position
/// honorifics from scratch, upsert the cached reporting.movie_honorifics row, and — only if the
/// recomputed honorific actually differs from what was previously stored — write a history row
/// and publish MovieHonorificEarnedIntegrationEvent (which is direction-agnostic: it carries both
/// the previous and new honorific values as plain ints, so a downgrade from a veto/CO reads the
/// same as an upgrade from a fresh pick; callers/consumers decide "earned" vs. "lost" framing by
/// comparing the two values).
///
/// Keeping this in one place means the UnifiedNumber1 / TheCycle math only has one home — changing
/// the rules later means changing them once, not once per handler.
/// </summary>
internal static class MovieHonorificRecompute
{
  public static async Task RunAsync(
    IDbConnection connection,
    string moviePublicId,
    string movieTitle,
    string draftPartPublicId,
    DateTime currentTime,
    IEventBus eventBus,
    CancellationToken cancellationToken
  )
  {
    const string picksSql = """
      SELECT board_position
      FROM reporting.movie_canonical_picks
      WHERE movie_public_id = @MoviePublicId;
      """;

    var positions = (
      await connection.QueryAsync<int>(
        new CommandDefinition(
          picksSql,
          new { MoviePublicId = moviePublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var appearanceCount = positions.Count;

    const string currentSql = """
      SELECT appearance_honorific, position_honorific
      FROM reporting.movie_honorifics
      WHERE movie_public_id = @MoviePublicId;
      """;

    var current = await connection.QuerySingleOrDefaultAsync<(
      int AppearanceHonorific,
      int PositionHonorific
    )>(
      new CommandDefinition(
        currentSql,
        new { MoviePublicId = moviePublicId },
        cancellationToken: cancellationToken
      )
    );

    var previousAppearanceHonorific = MovieHonorific.FromValue(current.AppearanceHonorific);
    var previousPositionHonorific = (MoviePositionHonorific)current.PositionHonorific;

    var newAppearanceHonorific = MovieHonorific.FromAppearanceCount(appearanceCount);
    var newPositionHonorific = MoviePositionHonorific.None;

    if (positions.Count(p => p == 1) >= 2)
    {
      newPositionHonorific |= MoviePositionHonorific.UnifiedNumber1;
    }

    var distinctPositions = positions.ToHashSet();
    if (
      distinctPositions.Contains(1)
      && distinctPositions.Contains(2)
      && distinctPositions.Contains(3)
      && distinctPositions.Contains(4)
    )
    {
      newPositionHonorific |= MoviePositionHonorific.TheCycle;
    }

    const string upsertSql = """
      INSERT INTO reporting.movie_honorifics
        (id, movie_public_id, movie_title, appearance_honorific, position_honorific, appearance_count, update_at_utc)
      VALUES
        (@Id, @MoviePublicId, @MovieTitle, @AppearanceHonorificValue, @PositionHonorific, @AppearanceCount, @UpdatedAt)
      ON CONFLICT (movie_public_id) DO UPDATE
        SET
          movie_title               = EXCLUDED.movie_title,
          appearance_honorific      = EXCLUDED.appearance_honorific,
          position_honorific        = EXCLUDED.position_honorific,
          appearance_count          = EXCLUDED.appearance_count,
          update_at_utc             = EXCLUDED.update_at_utc
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        upsertSql,
        new
        {
          Id = Guid.NewGuid(),
          MoviePublicId = moviePublicId,
          MovieTitle = movieTitle,
          AppearanceHonorificValue = newAppearanceHonorific.Value,
          PositionHonorific = (int)newPositionHonorific,
          AppearanceCount = appearanceCount,
          UpdatedAt = currentTime,
        },
        cancellationToken: cancellationToken
      )
    );

    var appearanceChanged = newAppearanceHonorific != previousAppearanceHonorific;
    var positionChanged = newPositionHonorific != previousPositionHonorific;

    if (!appearanceChanged && !positionChanged)
    {
      return;
    }

    const string historySql = """
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
          MoviePublicId = moviePublicId,
          AppearanceHonorificValue = newAppearanceHonorific.Value,
          PositionHonorific = (int)newPositionHonorific,
          AppearanceCount = appearanceCount,
          AchievedAt = currentTime,
        },
        cancellationToken: cancellationToken
      )
    );

    await eventBus.PublishAsync(
      new MovieHonorificEarnedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: currentTime,
        moviePublicId: moviePublicId,
        movieTitle: movieTitle,
        draftPartPublicId: draftPartPublicId,
        previousAppearanceHonorificValue: previousAppearanceHonorific.Value,
        newAppearanceHonorificValue: newAppearanceHonorific.Value,
        previousPositionHonorificValue: (int)previousPositionHonorific,
        newPositionHonorificValue: (int)newPositionHonorific,
        appearanceCount: appearanceCount
      ),
      cancellationToken
    );
  }
}
