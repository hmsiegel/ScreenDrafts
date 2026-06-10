// ═══════════════════════════════════════════════════════════════════════════════
// ListAttendances — GET /draft-parts/{draftPartId}/attendances
// Returns all attendance records for a draft part.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ListAttendances;

internal sealed record ListAttendancesQuery : IQuery<ListAttendancesResponse>
{
  public required string DraftPartId { get; init; }
}
