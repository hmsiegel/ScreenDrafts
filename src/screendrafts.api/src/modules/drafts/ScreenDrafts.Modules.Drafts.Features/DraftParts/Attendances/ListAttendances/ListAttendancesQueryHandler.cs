// ═══════════════════════════════════════════════════════════════════════════════
// ListAttendances — GET /draft-parts/{draftPartId}/attendances
// Returns all attendance records for a draft part.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ListAttendances;

internal sealed class ListAttendancesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListAttendancesQuery, ListAttendancesResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<ListAttendancesResponse>> Handle(
    ListAttendancesQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        a.public_id        AS {nameof(AttendanceItemResponse.PublicId)},
        a.person_public_id AS {nameof(AttendanceItemResponse.PersonPublicId)},
        a.status           AS {nameof(AttendanceItemResponse.Status)},
        a.created_at_utc   AS {nameof(AttendanceItemResponse.CreatedAtUtc)},
        a.updated_at_utc   AS {nameof(AttendanceItemResponse.UpdatedAtUtc)}
      FROM drafts.draft_part_attendances a
      JOIN drafts.draft_parts dp ON dp.id = a.draft_part_id
      WHERE dp.public_id = @DraftPartId
      ORDER BY a.created_at_utc ASC;
      """;

    var rows = await connection.QueryAsync<AttendanceItemResponse>(
      new CommandDefinition(sql, new { request.DraftPartId }, cancellationToken: cancellationToken)
    );

    var items = rows.Select(r => r with { StatusName = AttendanceStatus.FromValue(r.Status).Name })
      .ToList();

    return Result.Success(new ListAttendancesResponse { Items = items });
  }
}
