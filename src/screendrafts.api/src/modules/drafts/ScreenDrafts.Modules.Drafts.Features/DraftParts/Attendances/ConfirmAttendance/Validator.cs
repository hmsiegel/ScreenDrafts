// ═══════════════════════════════════════════════════════════════════════════════
// ConfirmAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/confirm
// Admin confirms person is attending. Pending → Confirmed.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ConfirmAttendance;

internal sealed class Validator : AbstractValidator<ConfirmAttendanceCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.PersonPublicId).NotEmpty();
  }
}
