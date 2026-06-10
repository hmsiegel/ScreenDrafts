// ═══════════════════════════════════════════════════════════════════════════════
// ReinstateAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/reinstate
// Admin reinstates a withdrawn record. Withdrawn → Pending.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ReinstateAttendance;

internal sealed record ReinstateAttendanceCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string PersonPublicId { get; init; }
}
