namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed class GetPredictionStandingsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetPredictionStandingsQuery, PredictionStandingsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PredictionStandingsResponse>> Handle(
    GetPredictionStandingsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // When AsOfDraftPartPublicId is supplied, points are computed by summing
    // prediction_results scored on or before the part's main-feed release date.
    // This gives the running total *as of that episode* rather than the full
    // season total written to prediction_standings.
    //
    // When not supplied (e.g. homepage), prediction_standings.points is used —
    // the full authoritative season total.

    string sql;

    if (request.AsOfDraftPartPublicId is not null)
    {
      sql = """
        WITH as_of_date AS (
          SELECT MAX(dr.release_date) AS cutoff
          FROM drafts.draft_parts    dp
          JOIN drafts.draft_releases dr ON dr.part_id  = dp.id
                                       AND dr.release_channel = @MainFeedReleaseChannel
          WHERE dp.public_id = @AsOfDraftPartPublicId
        ),
        episode_points AS (
          SELECT
            dps.season_id,
            dps.contestant_id,
            COALESCE(SUM(pr.points_awarded), 0) AS Points
          FROM drafts.draft_prediction_sets dps
          JOIN drafts.prediction_results    pr  ON pr.set_id         = dps.id
          JOIN drafts.draft_parts           dp2 ON dp2.id            = dps.draft_part_id
          JOIN drafts.draft_releases        dr2 ON dr2.part_id       = dp2.id
                                               AND dr2.release_channel = @MainFeedReleaseChannel
          CROSS JOIN as_of_date aod
          WHERE dr2.release_date <= aod.cutoff
          GROUP BY dps.season_id, dps.contestant_id
        )
        SELECT
          ps.public_id                                          AS SeasonPublicId,
          ps.number                                             AS SeasonNumber,
          ps.target_points                                      AS TargetPoints,
          ps.is_closed                                          AS IsClosed,
          c.public_id                                           AS ContestantPublicId,
          c.display_name                                        AS DisplayName,
          CAST(COALESCE(ep.Points, 0) AS INT)                   AS Points,
          CAST(COALESCE(SUM(co.points), 0) AS INT)              AS CarryoverPoints,
          st.first_crossed_target_at_utc                        AS FirstCrossedTargetAtUtc
        FROM drafts.prediction_seasons      ps
        LEFT JOIN drafts.prediction_standings   st ON st.season_id      = ps.id
        LEFT JOIN drafts.prediction_contestants c  ON c.id              = st.contestant_id
        LEFT JOIN drafts.prediction_carryovers  co ON co.season_id      = ps.id
                                                   AND co.contestant_id  = st.contestant_id
        LEFT JOIN episode_points               ep ON ep.season_id       = ps.id
                                                   AND ep.contestant_id  = st.contestant_id
        WHERE ps.public_id = @SeasonPublicId
        GROUP BY
          ps.public_id, ps.number, ps.target_points, ps.is_closed,
          c.public_id, c.display_name,
          ep.Points, st.first_crossed_target_at_utc
        ORDER BY (COALESCE(ep.Points, 0) + COALESCE(SUM(co.points), 0)) DESC;
        """;
    }
    else
    {
      sql = """
        SELECT
          ps.public_id                                          AS SeasonPublicId,
          ps.number                                             AS SeasonNumber,
          ps.target_points                                      AS TargetPoints,
          ps.is_closed                                          AS IsClosed,
          c.public_id                                           AS ContestantPublicId,
          c.display_name                                        AS DisplayName,
          CAST(COALESCE(st.points, 0) AS INT)                   AS Points,
          CAST(COALESCE(SUM(co.points), 0) AS INT)              AS CarryoverPoints,
          st.first_crossed_target_at_utc                        AS FirstCrossedTargetAtUtc
        FROM drafts.prediction_seasons      ps
        LEFT JOIN drafts.prediction_standings   st ON st.season_id      = ps.id
        LEFT JOIN drafts.prediction_contestants c  ON c.id              = st.contestant_id
        LEFT JOIN drafts.prediction_carryovers  co ON co.season_id      = ps.id
                                                   AND co.contestant_id  = st.contestant_id
        WHERE ps.public_id = @SeasonPublicId
        GROUP BY
          ps.public_id, ps.number, ps.target_points, ps.is_closed,
          c.public_id, c.display_name,
          st.points, st.first_crossed_target_at_utc
        ORDER BY (COALESCE(st.points, 0) + COALESCE(SUM(co.points), 0)) DESC;
        """;
    }
    var rows = (
      await connection.QueryAsync<StandingRow>(
        new CommandDefinition(
          sql,
          new
          {
            request.SeasonPublicId,
            request.AsOfDraftPartPublicId,
            MainFeedReleaseChannel = ReleaseChannel.MainFeed.Value,
          },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    if (rows.Count == 0)
    {
      return Result.Failure<PredictionStandingsResponse>(
        PredictionErrors.SeasonNotFound(request.SeasonPublicId)
      );
    }

    var first = rows[0];

    var standings = rows.Where(r => r.ContestantPublicId is not null)
      .Select(r => new ContestantStandingResponse
      {
        ContestantPublicId = r.ContestantPublicId!,
        DisplayName = r.DisplayName!,
        Points = r.Points,
        CarryoverPoints = r.CarryoverPoints,
        FirstCrossedTargetAtUtc = r.FirstCrossedTargetAtUtc,
        TotalPoints = r.Points + r.CarryoverPoints,
        HasCrossedTarget = r.FirstCrossedTargetAtUtc.HasValue,
      })
      .ToList();

    return Result.Success(
      new PredictionStandingsResponse
      {
        SeasonPublicId = first.SeasonPublicId,
        SeasonNumber = first.SeasonNumber,
        TargetPoints = first.TargetPoints,
        IsClosed = first.IsClosed,
        Standings = standings,
      }
    );
  }

  private sealed record StandingRow(
    string SeasonPublicId,
    int SeasonNumber,
    int TargetPoints,
    bool IsClosed,
    string? ContestantPublicId,
    string? DisplayName,
    int Points,
    int CarryoverPoints,
    DateTime? FirstCrossedTargetAtUtc
  );
}
