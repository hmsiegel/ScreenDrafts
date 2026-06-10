// ═══════════════════════════════════════════════════════════════════════════════
// JoinAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/join
// Person joins the draft part. Confirmed → Joined.
// Caller must be the person identified by personPublicId (or admin/commissioner).
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed record JoinAttendanceCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string PersonPublicId { get; init; }

  /// <summary>PublicId of the caller resolved from JWT, used for authorization check.</summary>
  public required string CallerPersonPublicId { get; init; }
}
