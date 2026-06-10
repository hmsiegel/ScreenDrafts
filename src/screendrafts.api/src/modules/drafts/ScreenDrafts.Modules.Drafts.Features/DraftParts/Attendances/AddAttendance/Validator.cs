// ═══════════════════════════════════════════════════════════════════════════════
// AddAttendance — POST /draft-parts/{draftPartId}/attendances
// Admin adds a person (drafter, host, or commissioner). Creates Pending record.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.AddAttendance;

internal sealed class Validator : AbstractValidator<AddAttendanceCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.PersonPublicId).NotEmpty();
  }
}
