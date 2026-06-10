// ═══════════════════════════════════════════════════════════════════════════════
// ListAttendances — GET /draft-parts/{draftPartId}/attendances
// Returns all attendance records for a draft part.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ListAttendances;

internal sealed record ListAttendancesResponse
{
  public IReadOnlyList<AttendanceItemResponse> Items { get; init; } = [];
}
