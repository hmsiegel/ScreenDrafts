namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraftPart;

internal sealed class PauseDraftPartCommandValidator : AbstractValidator<PauseDraftPartCommand>
{
  public PauseDraftPartCommandValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft Id cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft Id needs to be a valid GUID.");
      }
}
