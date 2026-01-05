namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ContinueDraftPart;

internal sealed class ContinueDraftPartCommandValidator : AbstractValidator<ContinueDraftPartCommand>
{
  public ContinueDraftPartCommandValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft Part ID cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft ID must be a valid GUID.");
  }
}
