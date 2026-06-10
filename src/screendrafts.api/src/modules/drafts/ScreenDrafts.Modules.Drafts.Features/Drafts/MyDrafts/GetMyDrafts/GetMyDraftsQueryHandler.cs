namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class GetMyDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMyDraftsQuery, GetMyDraftsResponse>
{
  private const int DraftStatusCreated = 0;
  private const int DraftStatusInProgress = 2;
  private const int DraftStatusPaused = 3;
  private const int DraftStatusCompleted = 4;
  private const int DraftStatusCancelled = 5;

  public async Task<Result<GetMyDraftsResponse>> Handle(
    GetMyDraftsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // All drafts where the caller is a drafter participant or a host on any part.
    const string draftsSql = $"""
      SELECT DISTINCT
        d.public_id    AS {nameof(DraftRow.DraftPublicId)},
        d.title        AS {nameof(DraftRow.Title)},
        d.draft_type   AS {nameof(DraftRow.DraftType)},
        d.draft_status AS {nameof(DraftRow.DraftStatus)},
        CASE WHEN dp2.id IS NOT NULL THEN true ELSE false END AS {nameof(DraftRow.HasPool)}
      FROM drafts.drafts d
      LEFT JOIN drafts.draft_pools dp2 ON dp2.draft_id = d.id
      WHERE
        -- caller is a drafter on at least one part
        EXISTS (
          SELECT 1
          FROM drafts.draft_part_participants dpp
          JOIN drafts.draft_parts part ON part.id = dpp.draft_part_id
          JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
                                  AND dpp.participant_kind_value = 0
          JOIN drafts.people pe ON pe.id = dr.person_id
          WHERE part.draft_id = d.id
            AND pe.user_id = @UserId
        )
        OR
        -- caller is a host on at least one part
        EXISTS (
          SELECT 1
          FROM drafts.draft_hosts dh
          JOIN drafts.draft_parts part ON part.id = dh.draft_part_id
          JOIN drafts.hosts h ON h.id = dh.host_id
          JOIN drafts.people pe ON pe.id = h.person_id
          WHERE part.draft_id = d.id
            AND pe.user_id = @UserId
        )
      ORDER BY d.title ASC;
      """;

    var draftRows = (
      await connection.QueryAsync<DraftRow>(
        new CommandDefinition(
          draftsSql,
          new { request.UserId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    if (draftRows.Count == 0)
      return Result.Success(new GetMyDraftsResponse());

    var draftPublicIds = draftRows.Select(d => d.DraftPublicId).ToList();

    // All parts for those drafts with their earliest release date.
    const string partsSql = $"""
      SELECT
        d.public_id   AS {nameof(PartRow.DraftPublicId)},
        dp.public_id  AS {nameof(PartRow.DraftPartPublicId)},
        dp.part_index AS {nameof(PartRow.PartIndex)},
        dp.status     AS {nameof(PartRow.Status)},
        MIN(r.release_date) AS {nameof(PartRow.ReleaseDate)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
      WHERE d.public_id = ANY(@DraftPublicIds)
      GROUP BY d.public_id, dp.public_id, dp.part_index, dp.status
      ORDER BY dp.part_index ASC;
      """;

    var partRows = (
      await connection.QueryAsync<PartRow>(
        new CommandDefinition(
          partsSql,
          new { DraftPublicIds = draftPublicIds.ToArray() },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var allPartPublicIds = partRows.Select(p => p.DraftPartPublicId).ToArray();

    // Parts where caller is a host.
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

    // Parts where caller is a drafter participant.
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

    // Attendance records for the caller across all these parts.
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

    // Assemble.
    var partsByDraft = partRows
      .GroupBy(p => p.DraftPublicId)
      .ToDictionary(
        g => g.Key,
        g =>
          g.Select(p => new MyDraftPartSummary
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
            })
            .ToList()
      );

    var summaries = draftRows
      .Select(d => new MyDraftSummary
      {
        DraftPublicId = d.DraftPublicId,
        Title = d.Title,
        DraftType = d.DraftType,
        DraftStatus = d.DraftStatus,
        HasPool = d.HasPool,
        Parts = partsByDraft.TryGetValue(d.DraftPublicId, out var parts) ? parts : [],
      })
      .ToList();

    var upcoming = summaries
      .Where(s => s.DraftStatus is DraftStatusCreated or DraftStatusPaused)
      .ToList();
    var inProgress = summaries.Where(s => s.DraftStatus == DraftStatusInProgress).ToList();
    var completed = summaries
      .Where(s => s.DraftStatus is DraftStatusCompleted or DraftStatusCancelled)
      .ToList();

    return Result.Success(
      new GetMyDraftsResponse
      {
        Upcoming = upcoming,
        InProgress = inProgress,
        Completed = completed,
      }
    );
  }

  private sealed record DraftRow(
    string DraftPublicId,
    string Title,
    int DraftType,
    int DraftStatus,
    bool HasPool
  );

  private sealed record PartRow(
    string DraftPublicId,
    string DraftPartPublicId,
    int PartIndex,
    int Status,
    DateOnly? ReleaseDate
  );
}
