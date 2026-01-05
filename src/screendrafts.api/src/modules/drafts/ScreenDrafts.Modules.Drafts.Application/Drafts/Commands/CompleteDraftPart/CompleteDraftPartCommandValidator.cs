namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraftPart;

internal sealed class CompleteDraftPartCommandValidator : AbstractValidator<CompleteDraftPartCommand>
{
  public CompleteDraftPartCommandValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft Part ID cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft ID must be a valid GUID.");
  }
}
