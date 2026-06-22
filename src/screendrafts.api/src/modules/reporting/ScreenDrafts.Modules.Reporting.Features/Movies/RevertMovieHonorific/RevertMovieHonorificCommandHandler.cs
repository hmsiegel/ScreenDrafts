namespace ScreenDrafts.Modules.Reporting.Features.Movies.RevertMovieHonorific;

internal sealed class RevertMovieHonorificCommandHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<RevertMovieHonorificCommand>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(RevertMovieHonorificCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var currentTime = _dateTimeProvider.UtcNow;

    const string deletePick =
      """
      DELETE FROM reporting.movie_canonical_picks
      WHERE movie_public_id = @MoviePublicId
        AND draft_part_public_id = @DraftPartPublicId;
      """;

    var deleted = await connection.ExecuteAsync(
      new CommandDefinition(
        deletePick,
        new
        {
          request.MoviePublicId,
          request.DraftPartPublicId
        },
        cancellationToken: cancellationToken));

    if (deleted == 0)
    {
      // Nothing to revert — the pick that was just vetoed / commissioner-overridden was never
      // canonical to begin with (e.g. CanonicalPolicyValue == 1, or == 2 with no main feed
      // release, per the same gate UpdateMovieHonorificCommandHandler's callers already check
      // before publishing PickLockedIntegrationEvent in the first place). VetoApplied /
      // CommissionerOverrideApplied intentionally publish PickUnlockedIntegrationEvent
      // unconditionally rather than re-deriving that gate, so this is the expected, harmless
      // no-op absorbing case — not an error.
      return Result.Success();
    }

    await MovieHonorificRecompute.RunAsync(
      connection,
      request.MoviePublicId,
      request.MovieTitle,
      request.DraftPartPublicId,
      currentTime,
      _eventBus,
      cancellationToken);

    return Result.Success();
  }
}
