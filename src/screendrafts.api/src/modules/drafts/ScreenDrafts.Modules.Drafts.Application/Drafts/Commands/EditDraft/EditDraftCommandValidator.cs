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
    RuleFor(x => x.TotalPicks)
      .GreaterThan(0).WithMessage("Total picks must be greater than zero.");
    RuleFor(x => x.TotalDrafters)
      .GreaterThan(0).WithMessage("Total drafters must be greater than zero.");
    RuleFor(x => x.TotalDrafterTeams)
      .GreaterThan(0).WithMessage("Total drafter teams must be greater than zero.");
    RuleFor(x => x.TotalHosts)
      .GreaterThanOrEqualTo(0).WithMessage("Total hosts must be zero or greater.");
    RuleFor(x => x.EpisodeType)
      .IsInEnum().WithMessage("Invalid episode type.");
    RuleFor(x => x.DraftStatus)
      .IsInEnum().WithMessage("Invalid draft status.");
  }
}
