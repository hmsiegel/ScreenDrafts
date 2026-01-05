namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.EditDraft;

internal sealed class EditDraftCommandValidator : AbstractValidator<EditDraftCommand>
{
  public EditDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("Draft ID cannot be empty.")
      .Must(id => id.BeValidGuid()).WithMessage("Draft ID should be a valid GUID.");
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Title cannot be empty.")
      .MaximumLength(Title.MaxLength).WithMessage($"Title cannot exceed {Title.MaxLength} characters.");
    RuleFor(x => x.DraftType)
      .IsInEnum().WithMessage("Invalid draft type.");
  }
}
