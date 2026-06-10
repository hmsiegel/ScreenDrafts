// ═══════════════════════════════════════════════════════════════════════════════
// AddAttendance — POST /draft-parts/{draftPartId}/attendances
// Admin adds a person (drafter, host, or commissioner). Creates Pending record.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.AddAttendance;

internal sealed record AddAttendanceResponse
{
  public string PublicId { get; init; } = default!;
  public string PersonPublicId { get; init; } = default!;
  public string Status { get; init; } = default!;
}
