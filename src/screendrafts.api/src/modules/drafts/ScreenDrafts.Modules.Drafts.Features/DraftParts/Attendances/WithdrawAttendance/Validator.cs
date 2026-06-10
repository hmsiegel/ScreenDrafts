// ═══════════════════════════════════════════════════════════════════════════════
// WithdrawAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/withdraw
// Person or admin withdraws. Any non-Withdrawn status → Withdrawn.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.WithdrawAttendance;

internal sealed class Validator : AbstractValidator<WithdrawAttendanceCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.PersonPublicId).NotEmpty();
    RuleFor(x => x.CallerPersonPublicId).NotEmpty();
  }
}
