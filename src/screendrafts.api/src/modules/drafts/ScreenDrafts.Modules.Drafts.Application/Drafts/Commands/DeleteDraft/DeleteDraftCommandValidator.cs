namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.DeleteDraft;

internal sealed class DeleteDraftCommandValidator : AbstractValidator<DeleteDraftCommand>
{
  public DeleteDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
        .NotEmpty().WithMessage("Draft ID must not be empty.")
        .Must(id => id.BeValidGuid()).WithMessage("Draft ID should be a valid GUID.");
  }
}
