namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed class GetDraftPartPredictionsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetDraftPartPredictionsQuery, IReadOnlyList<DraftPartPredictionResponse>>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<IReadOnlyList<DraftPartPredictionResponse>>> Handle(
    GetDraftPartPredictionsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        s.public_id            AS SetPublicId,
        c.public_id            AS ContestantPublicId,
        c.display_name         AS ContestantDisplayName,
        s.submitted_at_utc     AS SubmittedAtUtc,
        s.source_kind          AS SourceKind,
        s.locked_at_utc        AS LockedAtUtc,
        e.media_public_id      AS MediaPublicId,
        e.media_title          AS MediaTitle,
        e.order_index          AS OrderIndex,
        e.notes                AS Notes,
        r.correct_count        AS CorrectCount,
        r.shoot_the_moon       AS ShootsTheMoon,
        r.points_awarded       AS PointsAwarded,
        r.scored_at_utc        AS ScoredAtUtc
      FROM drafts.draft_prediction_sets s
      JOIN drafts.draft_parts            dp ON dp.id         = s.draft_part_id
      JOIN drafts.prediction_contestants c  ON c.id          = s.contestant_id
      LEFT JOIN drafts.prediction_entries e  ON e.set_id     = s.id
      LEFT JOIN drafts.prediction_results r  ON r.set_id     = s.id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY c.display_name, e.order_index;
      """;

    var rows = (await connection.QueryAsync<SetRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { request.DraftPartPublicId },
        cancellationToken: cancellationToken))).ToList();

    var response = rows
      .GroupBy(r => r.SetPublicId)
      .Select(g =>
      {
        var first = g.First();

        var entries = g
          .Where(r => r.MediaPublicId is not null)
          .Select(r => new PredictionEntryResponse
          {
            MediaPublicId = r.MediaPublicId!,
            MediaTitle = r.MediaTitle!,
            OrderIndex = r.OrderIndex!,
            Notes = r.Notes
          }).ToList();

        PredictionResultResponse? result = first.CorrectCount.HasValue
          ? new PredictionResultResponse
          {
            CorrectCount = first.CorrectCount.Value,
            ShootsTheMoon = first.ShootsTheMoon!.Value,
            PointsAwarded = first.PointsAwarded!.Value,
            ScoredAtUtc = first.ScoredAtUtc!.Value
          }
          : null;

        return new DraftPartPredictionResponse
        {
          PublicId = first.SetPublicId,
          ContestantPublicId = first.ContestantPublicId,
          ContestantDisplayName = first.ContestantDisplayName,
          SubmittedAtUtc = first.SubmittedAtUtc,
          SourceKind = PredictionSourceKind.FromValue(first.SourceKind).Name,
          IsLocked = first.LockedAtUtc.HasValue,
          LockedAtUtc = first.LockedAtUtc,
          Entries = entries,
          Result = result
        };
      }).ToList();

    return Result.Success<IReadOnlyList<DraftPartPredictionResponse>>(response);
  }

  private sealed record SetRow(
    string SetPublicId,
    string ContestantPublicId,
    string ContestantDisplayName,
    DateTime SubmittedAtUtc,
    int SourceKind,
    DateTime? LockedAtUtc,
    string? MediaPublicId,
    string? MediaTitle,
    int? OrderIndex,
    string? Notes,
    int? CorrectCount,
    bool? ShootsTheMoon,
    int? PointsAwarded,
    DateTime? ScoredAtUtc);
}
