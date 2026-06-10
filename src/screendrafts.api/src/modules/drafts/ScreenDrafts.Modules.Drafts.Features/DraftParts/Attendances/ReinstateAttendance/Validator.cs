// ═══════════════════════════════════════════════════════════════════════════════
// ReinstateAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/reinstate
// Admin reinstates a withdrawn record. Withdrawn → Pending.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ReinstateAttendance;

internal sealed class Validator : AbstractValidator<ReinstateAttendanceCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.PersonPublicId).NotEmpty();
  }
}
