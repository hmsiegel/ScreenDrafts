namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed class GetPredictionStandingsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetPredictionStandingsQuery, PredictionStandingsResponse>
{
  public async Task<Result<PredictionStandingsResponse>> Handle(
    GetPredictionStandingsQuery request,
    CancellationToken cancellationToken)
  {
    using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        ps.public_id                      AS SeasonPublicId,
        ps.number                         AS SeasonNumber,
        ps.target_points                  AS TargetPoints,
        ps.is_closed                      AS IsClosed,
        c.public_id                       AS ContestantPublicId,
        c.display_name                    AS DisplayName,
        COALESCE(st.points, 0)            AS Points,
        COALESCE(SUM(co.points), 0)       AS CarryoverPoints,
        st.first_crossed_target_at_utc    AS FirstCrossedTargetAtUtc
      FROM drafts.prediction_seasons ps
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

    var rows = (await connection.QueryAsync<StandingRow>(
      sql,
      new { request.SeasonPublicId })).ToList();

    if (rows.Count == 0)
    {
      return Result.Failure<PredictionStandingsResponse>(
        PredictionErrors.SeasonNotFound(request.SeasonPublicId));
    }

    var first = rows[0];

    var standings = rows
      .Where(r => r.ContestantPublicId is not null)
      .Select(r => new ContestantStandingResponse
      {
        ContestantPublicId = r.ContestantPublicId!,
        DisplayName = r.DisplayName!,
        Points = r.Points,
        CarryoverPoints = (int)r.CarryoverPoints,
        TotalPoints = r.Points + (int)r.CarryoverPoints,
        HasCrossedTarget = r.FirstCrossedTargetAtUtc.HasValue,
        FirstCrossedTargetAtUtc = r.FirstCrossedTargetAtUtc
      })
      .ToList();

    return Result.Success(new PredictionStandingsResponse
    {
      SeasonPublicId = first.SeasonPublicId,
      SeasonNumber = first.SeasonNumber,
      TargetPoints = first.TargetPoints,
      IsClosed = first.IsClosed,
      Standings = standings
    });
  }

  private sealed record StandingRow(
    string SeasonPublicId,
    int SeasonNumber,
    int TargetPoints,
    bool IsClosed,
    string? ContestantPublicId,
    string? DisplayName,
    int Points,
    long CarryoverPoints,
    DateTime? FirstCrossedTargetAtUtc);

}
