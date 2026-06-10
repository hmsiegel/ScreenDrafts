// ═══════════════════════════════════════════════════════════════════════════════
// ReinstateAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/reinstate
// Admin reinstates a withdrawn record. Withdrawn → Pending.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ReinstateAttendance;

internal sealed record ReinstateAttendanceRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "personPublicId")]
  public string PersonPublicId { get; init; } = default!;
}
