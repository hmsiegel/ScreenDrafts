namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetCurrentPredictionSeason;

internal sealed class GetCurrentPredictionSeasonQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetCurrentPredictionSeasonQuery, PredictionSeasonSummaryResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PredictionSeasonSummaryResponse>> Handle(
    GetCurrentPredictionSeasonQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      WITH season_episodes AS (
        SELECT
          dps.season_id,
          MIN(dcr.episode_number) AS FirstEpisodeNumber,
          MAX(dcr.episode_number) AS LastEpisodeNumber
        FROM drafts.draft_prediction_sets  dps
        JOIN drafts.draft_parts            dp  ON dp.id      = dps.draft_part_id
        JOIN drafts.drafts                 d   ON d.id       = dp.draft_id
        JOIN drafts.draft_channel_releases dcr ON dcr.draft_id = d.id
                                              AND dcr.release_channel = @MainFeedReleaseChannel
        WHERE dcr.episode_number IS NOT NULL
        GROUP BY dps.season_id
      )
      SELECT
        ps.public_id                      AS SeasonPublicId,
        ps.number                         AS SeasonNumber,
        ps.starts_on                      AS StartsOn,
        ps.ends_on                        AS EndsOn,
        ps.target_points                  AS TargetPoints,
        ps.is_closed                      AS IsClosed,
        se.FirstEpisodeNumber             AS FirstEpisodeNumber,
        se.LastEpisodeNumber              AS LastEpisodeNumber,
        c.public_id                       AS ContestantPublicId,
        c.display_name                    AS DisplayName,
        COALESCE(st.points, 0)            AS Points,
        st.first_crossed_target_at_utc    AS FirstCrossedTargetAtUtc
      FROM drafts.prediction_seasons ps
      LEFT JOIN season_episodes           se ON se.season_id  = ps.id
      LEFT JOIN drafts.prediction_standings   st ON st.season_id     = ps.id
      LEFT JOIN drafts.prediction_contestants c  ON c.id             = st.contestant_id
      WHERE ps.is_closed = FALSE
      GROUP BY
        ps.public_id, ps.number, ps.starts_on, ps.ends_on,
        ps.target_points, ps.is_closed,
        se.FirstEpisodeNumber, se.LastEpisodeNumber,
        c.public_id, c.display_name,
        st.points, st.first_crossed_target_at_utc
      ORDER BY (COALESCE(st.points, 0)) DESC;
      """;

    var rows = (await connection.QueryAsync<SeasonRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { MainFeedReleaseChannel = ReleaseChannel.MainFeed.Value },
        cancellationToken: cancellationToken))).ToList();

    if (rows.Count == 0)
    {
      return Result.Failure<PredictionSeasonSummaryResponse>(
        PredictionErrors.SeasonNotFound("current"));
    }

    var first = rows[0];

    var standings = rows
      .Where(r => r.ContestantPublicId is not null)
      .Select(r => new SeasonContestantStandingResponse
      {
        ContestantPublicId = r.ContestantPublicId!,
        DisplayName = r.DisplayName!,
        Points = r.Points,
        HasCrossedTarget = r.FirstCrossedTargetAtUtc.HasValue,
        FirstCrossedTargetAtUtc = r.FirstCrossedTargetAtUtc
      })
      .ToList();

    return Result.Success(new PredictionSeasonSummaryResponse
    {
      PublicId = first.SeasonPublicId,
      Number = first.SeasonNumber,
      StartDate = first.StartsOn,
      EndDate = first.EndsOn,
      FirstEpisodeNumber = first.FirstEpisodeNumber,
      LastEpisodeNumber = first.LastEpisodeNumber,
      TargetPoints = first.TargetPoints,
      IsClosed = first.IsClosed,
      Standings = standings
    });
  }

  private sealed record SeasonRow
  {
    public string SeasonPublicId { get; init; } = default!;
    public int SeasonNumber { get; init; } = default!;
    public DateOnly StartsOn { get; init; } = default!;
    public DateOnly? EndsOn { get; init; } = default!;
    public int TargetPoints { get; init; } = default!;
    public bool IsClosed { get; init; } = default!;
    public int? FirstEpisodeNumber { get; init; } = default!;
    public int? LastEpisodeNumber { get; init; } = default!;
    public string? ContestantPublicId { get; init; } = default!;
    public string? DisplayName { get; init; } = default!;
    public int Points { get; init; } = default!;
    public DateTime? FirstCrossedTargetAtUtc { get; init; } = default!;
  }
}
