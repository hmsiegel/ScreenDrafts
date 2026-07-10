namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class GetMyDraftDetailQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMyDraftDetailQuery, GetMyDraftDetailResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMyDraftDetailResponse>> Handle(
    GetMyDraftDetailQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // ── Draft header ──────────────────────────────────────────────────────────

    const string headerSql = $"""
      SELECT
        d.public_id  AS {nameof(DraftHeaderRow.DraftPublicId)},
        d.title      AS {nameof(DraftHeaderRow.Title)},
        d.draft_type AS {nameof(DraftHeaderRow.DraftType)},
        CASE WHEN pool.id IS NOT NULL THEN true ELSE false END AS {nameof(DraftHeaderRow.HasPool)}
      FROM drafts.drafts d
      LEFT JOIN drafts.draft_pools pool ON pool.draft_id = d.id
      WHERE d.public_id = @DraftId
      LIMIT 1;
      """;

    var header = await connection.QuerySingleOrDefaultAsync<DraftHeaderRow>(
      new CommandDefinition(
        headerSql,
        new { request.DraftId },
        cancellationToken: cancellationToken
      )
    );

    if (header is null)
      return Result.Failure<GetMyDraftDetailResponse>(DraftErrors.NotFound(request.DraftId));

    // ── Parts with earliest release date ─────────────────────────────────────

    const string partsSql = $"""
      SELECT
        dp.public_id  AS {nameof(PartDetailRow.DraftPartPublicId)},
        dp.part_index AS {nameof(PartDetailRow.PartIndex)},
        dp.status     AS {nameof(PartDetailRow.Status)},
        MIN(r.release_date) AS {nameof(PartDetailRow.ReleaseDate)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
      WHERE d.public_id = @DraftId
      GROUP BY dp.public_id, dp.part_index, dp.status
      ORDER BY dp.part_index ASC;
      """;

    var partRows = (
      await connection.QueryAsync<PartDetailRow>(
        new CommandDefinition(
          partsSql,
          new { request.DraftId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var allPartPublicIds = partRows.Select(p => p.DraftPartPublicId).ToArray();

    // ── Parts where caller is a host ──────────────────────────────────────────

    const string hostPartsSql = $"""
      SELECT dp.public_id
      FROM drafts.draft_hosts dh
      JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people pe ON pe.id = h.person_id
      WHERE pe.user_id = @UserId
        AND dp.public_id = ANY(@PartPublicIds);
      """;

    var hostPartIds = (
      await connection.QueryAsync<string>(
        new CommandDefinition(
          hostPartsSql,
          new { request.UserId, PartPublicIds = allPartPublicIds },
          cancellationToken: cancellationToken
        )
      )
    ).ToHashSet();

    // ── Parts where caller is a drafter ───────────────────────────────────────

    const string drafterPartsSql = $"""
      SELECT dp.public_id
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
                              AND dpp.participant_kind_value = 0
      JOIN drafts.people pe ON pe.id = dr.person_id
      WHERE pe.user_id = @UserId
        AND dp.public_id = ANY(@PartPublicIds);
      """;

    var drafterPartIds = (
      await connection.QueryAsync<string>(
        new CommandDefinition(
          drafterPartsSql,
          new { request.UserId, PartPublicIds = allPartPublicIds },
          cancellationToken: cancellationToken
        )
      )
    ).ToHashSet();

    // ── Attendance records for caller ─────────────────────────────────────────

    const string attendanceSql = $"""
      SELECT
        dp.public_id AS PartPublicId,
        a.status     AS AttendanceStatus
      FROM drafts.draft_part_attendances a
      JOIN drafts.draft_parts dp ON dp.id = a.draft_part_id
      WHERE a.person_public_id = (
        SELECT pe.public_id
        FROM drafts.people pe
        WHERE pe.user_id = @UserId
        LIMIT 1
      )
      AND dp.public_id = ANY(@PartPublicIds);
      """;

    var attendanceMap = (
      await connection.QueryAsync<(string PartPublicId, int AttendanceStatus)>(
        new CommandDefinition(
          attendanceSql,
          new { request.UserId, PartPublicIds = allPartPublicIds },
          cancellationToken: cancellationToken
        )
      )
    ).ToDictionary(x => x.PartPublicId, x => x.AttendanceStatus);

    // ── Predictor eligibility ─────────────────────────────────────────────────
    // Caller qualifies for a slot as either the contestant themself or that
    // slot's designated submitter (drafts.draft_part_predictors). Distinct from
    // the surrogate_assignments check below, which is a scoring-time merge
    // between two already-submitted sets, not a submission-time permission.

    const string predictorSql = $"""
      SELECT
        dp.public_id AS {nameof(PredictorRow.PartPublicId)},
        pc.public_id AS {nameof(PredictorRow.ContestantPublicId)},
        EXISTS (
          SELECT 1
          FROM drafts.draft_prediction_sets s
          WHERE s.contestant_id = pc.id
            AND s.draft_part_id = dp.id
        ) AS {nameof(PredictorRow.HasSubmitted)}
      FROM drafts.draft_part_predictors dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.prediction_contestants pc ON pc.id = dpp.contestant_id
      LEFT JOIN drafts.people contestant_person ON contestant_person.id = pc.person_id
      LEFT JOIN drafts.people submitter_person ON submitter_person.id = dpp.allowed_submitter_person_id
      WHERE dp.public_id = ANY(@PartPublicIds)
        AND (contestant_person.user_id = @UserId OR submitter_person.user_id = @UserId);
      """;

    var predictorMap = (
      await connection.QueryAsync<PredictorRow>(
        new CommandDefinition(
          predictorSql,
          new { request.UserId, PartPublicIds = allPartPublicIds },
          cancellationToken: cancellationToken
        )
      )
    ).ToDictionary(x => x.PartPublicId);

    // ── isSurrogate check ─────────────────────────────────────────────────────
    // True when a SurrogateAssignment exists where the surrogate set's
    // submitted_by_person has user_id = caller, and that set belongs to a part
    // of this draft.

    const string surrogateSql = $"""
      SELECT EXISTS (
        SELECT 1
        FROM drafts.surrogate_assignments sa
        JOIN drafts.draft_prediction_sets surrogate_set
          ON sa.surrogate_set_id = surrogate_set.id
        JOIN drafts.draft_parts dp
          ON surrogate_set.draft_part_id = dp.id
        JOIN drafts.drafts d
          ON dp.draft_id = d.id
        JOIN drafts.people pe
          ON surrogate_set.submitted_by_person_id = pe.id
        WHERE d.public_id = @DraftId
          AND pe.user_id = @UserId
      );
      """;

    var isSurrogate = await connection.ExecuteScalarAsync<bool>(
      new CommandDefinition(
        surrogateSql,
        new { request.DraftId, request.UserId },
        cancellationToken: cancellationToken
      )
    );

    // ── Verify caller has any role on this draft ──────────────────────────────

    var isHost = hostPartIds.Count > 0;
    var isDrafter = drafterPartIds.Count > 0;
    var isPredictor = predictorMap.Count > 0;

    if (!isHost && !isDrafter && !isPredictor && !request.IsAdmin)
      return Result.Failure<GetMyDraftDetailResponse>(DraftErrors.NotFound(request.DraftId));

    // ── Assemble parts ────────────────────────────────────────────────────────

    var parts = partRows
      .Select(p =>
      {
        predictorMap.TryGetValue(p.DraftPartPublicId, out var predictor);

        return new MyDraftPartDetail
        {
          DraftPartPublicId = p.DraftPartPublicId,
          PartIndex = p.PartIndex,
          Status = p.Status,
          IsHost = hostPartIds.Contains(p.DraftPartPublicId),
          IsDrafter = drafterPartIds.Contains(p.DraftPartPublicId),
          AttendanceStatus = attendanceMap.TryGetValue(p.DraftPartPublicId, out var s)
            ? AttendanceStatus.FromValue(s).Name
            : null,
          ReleaseDate = p.ReleaseDate,
          IsPredictor = predictor is not null,
          ContestantPublicId = predictor?.ContestantPublicId,
          HasSubmittedPrediction = predictor?.HasSubmitted ?? false,
        };
      })
      .ToList();

    // ── Roles ─────────────────────────────────────────────────────────────────

    var roles = new List<string>();
    if (isHost)
      roles.Add("Host");
    if (isDrafter)
      roles.Add("Drafter");

    return Result.Success(
      new GetMyDraftDetailResponse
      {
        DraftPublicId = header.DraftPublicId,
        Title = header.Title,
        DraftType = header.DraftType,
        HasPool = header.HasPool,
        IsSurrogate = isSurrogate,
        MyRoles = roles,
        Parts = parts,
      }
    );
  }

  private sealed record DraftHeaderRow(
    string DraftPublicId,
    string Title,
    int DraftType,
    bool HasPool
  );

  private sealed record PartDetailRow(
    string DraftPartPublicId,
    int PartIndex,
    int Status,
    DateOnly? ReleaseDate
  );

  private sealed record PredictorRow(
    string PartPublicId,
    string ContestantPublicId,
    bool HasSubmitted
  );
}
