// ═══════════════════════════════════════════════════════════════════════════════
// ConfirmAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/confirm
// Admin confirms person is attending. Pending → Confirmed.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ConfirmAttendance;

internal sealed record ConfirmAttendanceCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string PersonPublicId { get; init; }
}
