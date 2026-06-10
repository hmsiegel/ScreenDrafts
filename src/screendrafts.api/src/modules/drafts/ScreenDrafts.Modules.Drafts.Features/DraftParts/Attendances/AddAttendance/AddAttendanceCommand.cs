// ═══════════════════════════════════════════════════════════════════════════════
// AddAttendance — POST /draft-parts/{draftPartId}/attendances
// Admin adds a person (drafter, host, or commissioner). Creates Pending record.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.AddAttendance;

internal sealed record AddAttendanceCommand : ICommand<AddAttendanceResponse>
{
  public required string DraftPartId { get; init; }
  public required string PersonPublicId { get; init; }
}
