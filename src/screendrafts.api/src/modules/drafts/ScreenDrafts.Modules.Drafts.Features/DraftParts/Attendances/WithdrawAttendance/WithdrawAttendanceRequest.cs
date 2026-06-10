// ═══════════════════════════════════════════════════════════════════════════════
// WithdrawAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/withdraw
// Person or admin withdraws. Any non-Withdrawn status → Withdrawn.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.WithdrawAttendance;

internal sealed record WithdrawAttendanceRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "personPublicId")]
  public string PersonPublicId { get; init; } = default!;
}
