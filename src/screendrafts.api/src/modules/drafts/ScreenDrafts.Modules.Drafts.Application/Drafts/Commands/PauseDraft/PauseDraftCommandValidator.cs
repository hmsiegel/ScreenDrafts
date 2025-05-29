namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraft;

internal sealed class PauseDraftCommandValidator : AbstractValidator<PauseDraftCommand>
{
  public PauseDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("Draft Id cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft Id needs to be a valid GUID.");
      }
}
