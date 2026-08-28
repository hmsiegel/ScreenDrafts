namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed class ListPredictionSeasonsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<ListPredictionSeasonsQuery, ListPredictionSeasonsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<ListPredictionSeasonsResponse>> Handle(
    ListPredictionSeasonsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string seasonsSql = """
      SELECT
        ps.id                AS InternalId,
        ps.public_id          AS PublicId,
        ps.number             AS Number,
        ps.starts_on          AS StartsOn,
        ps.ends_on            AS EndsOn,
        ps.target_points      AS TargetPoints,
        ps.is_closed          AS IsClosed
      FROM drafts.prediction_seasons ps
      ORDER BY ps.number DESC
      """;

    var seasonRows = (
      await connection.QueryAsync<SeasonRow>(
        new CommandDefinition(seasonsSql, cancellationToken: cancellationToken)
      )
    ).ToList();

    if (seasonRows.Count == 0)
    {
      return Result.Success(new ListPredictionSeasonsResponse { Seasons = [] });
    }

    const string baseDraftsSql = $"""
      SELECT DISTINCT
        ps.id                  AS {nameof(DraftRow.SeasonInternalId)},
        d.public_id            AS {nameof(DraftRow.DraftPublicId)},
        dp.public_id           AS {nameof(DraftRow.DraftPartPublicId)},
        CASE
          WHEN (SELECT COUNT(*) FROM drafts.draft_parts dp2 WHERE dp2.draft_id = d.id) > 1
            THEN CONCAT(d.title, ' - Part ', dp.part_index)
          ELSE d.title
        END                    AS {nameof(DraftRow.Label)},
        dcr.episode_number     AS {nameof(DraftRow.EpisodeNumber)}
      FROM drafts.draft_prediction_sets dps
      JOIN drafts.prediction_seasons ps ON ps.id = dps.season_id
      JOIN drafts.draft_parts dp        ON dp.id = dps.draft_part_id
      JOIN drafts.drafts d              ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_channel_releases dcr
        ON dcr.draft_id = d.id AND dcr.release_channel = @MainFeedChannel
      WHERE d.is_deleted = FALSE
      """;

    var sqlBuilder = new StringBuilder(baseDraftsSql);

    if (!request.IncludePatreon)
    {
      sqlBuilder.Append(
        """

        AND NOT EXISTS (
          SELECT 1
          FROM drafts.draft_releases dr2
          WHERE dr2.part_id = dp.id
          AND dr2.release_channel = @PatreonChannel
          AND NOT EXISTS (
            SELECT 1
            FROM drafts.draft_releases dr3
            WHERE dr3.part_id = dp.id
            AND dr3.release_channel = @MainFeedChannel
          )
        )
        """
      );
    }

    sqlBuilder.Append(
      """

      ORDER BY EpisodeNumber DESC NULLS LAST
      """
    );

    var draftRows = (
      await connection.QueryAsync<DraftRow>(
        new CommandDefinition(
          sqlBuilder.ToString(),
          new
          {
            MainFeedChannel = ReleaseChannel.MainFeed.Value,
            PatreonChannel = ReleaseChannel.Patreon.Value,
          },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var draftPartPublicIds = draftRows.Select(r => r.DraftPartPublicId).Distinct().ToList();

    var scoresByDraftPartPublicId =
      new Dictionary<string, List<PredictionSeasonDraftScoreResponse>>();

    if (draftPartPublicIds.Count > 0)
    {
      const string scoresSql = $"""
        SELECT
          dp.public_id       AS {nameof(ScoreRow.DraftPartPublicId)},
          c.display_name     AS {nameof(ScoreRow.ContestantDisplayName)},
          r.points_awarded   AS {nameof(ScoreRow.PointsAwarded)}
        FROM drafts.draft_prediction_sets dps
        JOIN drafts.draft_parts dp               ON dp.id = dps.draft_part_id
        JOIN drafts.prediction_contestants c     ON c.id = dps.contestant_id
        JOIN drafts.prediction_results r         ON r.set_id = dps.id
        WHERE dp.public_id = ANY(@DraftPartPublicIds)
        ORDER BY c.display_name
        """;

      var scoreRows = (
        await connection.QueryAsync<ScoreRow>(
          new CommandDefinition(
            scoresSql,
            new { DraftPartPublicIds = draftPartPublicIds },
            cancellationToken: cancellationToken
          )
        )
      ).ToList();

      scoresByDraftPartPublicId = scoreRows
        .GroupBy(r => r.DraftPartPublicId)
        .ToDictionary(
          g => g.Key,
          g =>
            g.Select(r => new PredictionSeasonDraftScoreResponse
              {
                ContestantDisplayName = r.ContestantDisplayName,
                PointsAwarded = r.PointsAwarded,
              })
              .ToList()
        );
    }

    var draftsBySeasonId = draftRows
      .GroupBy(r => r.SeasonInternalId)
      .ToDictionary(
        g => g.Key,
        g =>
          (IReadOnlyList<PredictionSeasonDraftResponse>)
            [
              .. g.Select(r => new PredictionSeasonDraftResponse
              {
                DraftPublicId = r.DraftPublicId,
                DraftPartPublicId = r.DraftPartPublicId,
                Label = r.Label,
                EpisodeNumber = r.EpisodeNumber,
                Scores = scoresByDraftPartPublicId.GetValueOrDefault(r.DraftPartPublicId, []),
              }),
            ]
      );

    var seasons = seasonRows
      .Select(s =>
      {
        var item = new PredictionSeasonListItemResponse
        {
          PublicId = s.PublicId,
          Number = s.Number,
          StartsOn = s.StartsOn,
          EndsOn = s.EndsOn,
          TargetPoints = s.TargetPoints,
          IsClosed = s.IsClosed,
        };

        item.SetDrafts(draftsBySeasonId.GetValueOrDefault(s.InternalId, []));

        return item;
      })
      .ToList();

    return Result.Success(new ListPredictionSeasonsResponse { Seasons = seasons });
  }

  private sealed record SeasonRow(
    Guid InternalId,
    string PublicId,
    int Number,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    int TargetPoints,
    bool IsClosed
  );

  private sealed record DraftRow(
    Guid SeasonInternalId,
    string DraftPublicId,
    string DraftPartPublicId,
    string Label,
    int? EpisodeNumber
  );

  private sealed record ScoreRow(
    string DraftPartPublicId,
    string ContestantDisplayName,
    int PointsAwarded
  );
}
