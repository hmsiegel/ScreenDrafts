namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ContinueDraft;

internal sealed class ContinueDraftCommandValidator : AbstractValidator<ContinueDraftCommand>
{
  public ContinueDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("Draft ID cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft ID must be a valid GUID.");
  }
}
