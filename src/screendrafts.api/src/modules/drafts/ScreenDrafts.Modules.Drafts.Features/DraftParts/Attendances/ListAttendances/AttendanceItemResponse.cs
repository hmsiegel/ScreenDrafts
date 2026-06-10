// ═══════════════════════════════════════════════════════════════════════════════
// ListAttendances — GET /draft-parts/{draftPartId}/attendances
// Returns all attendance records for a draft part.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ListAttendances;

internal sealed record AttendanceItemResponse
{
  public string PublicId { get; init; } = default!;
  public string PersonPublicId { get; init; } = default!;
  public int Status { get; init; }
  public string StatusName { get; init; } = default!;
  public DateTime CreatedAtUtc { get; init; }
  public DateTime? UpdatedAtUtc { get; init; }
}
