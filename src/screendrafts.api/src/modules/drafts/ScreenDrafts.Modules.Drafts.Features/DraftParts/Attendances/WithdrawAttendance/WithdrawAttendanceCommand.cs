// ═══════════════════════════════════════════════════════════════════════════════
// WithdrawAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/withdraw
// Person or admin withdraws. Any non-Withdrawn status → Withdrawn.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.WithdrawAttendance;

internal sealed record WithdrawAttendanceCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string PersonPublicId { get; init; }
  public required string CallerPersonPublicId { get; init; }
}
