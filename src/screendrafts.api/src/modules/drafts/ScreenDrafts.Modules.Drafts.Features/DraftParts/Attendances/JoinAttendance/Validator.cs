// ═══════════════════════════════════════════════════════════════════════════════
// JoinAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/join
// Person joins the draft part. Confirmed → Joined.
// Caller must be the person identified by personPublicId (or admin/commissioner).
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed class Validator : AbstractValidator<JoinAttendanceCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
  }
}
