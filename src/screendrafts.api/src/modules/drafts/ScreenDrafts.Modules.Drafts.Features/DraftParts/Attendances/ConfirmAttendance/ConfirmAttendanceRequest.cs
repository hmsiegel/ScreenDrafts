// ═══════════════════════════════════════════════════════════════════════════════
// ConfirmAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/confirm
// Admin confirms person is attending. Pending → Confirmed.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ConfirmAttendance;

internal sealed record ConfirmAttendanceRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "personPublicId")]
  public string PersonPublicId { get; init; } = default!;
}
