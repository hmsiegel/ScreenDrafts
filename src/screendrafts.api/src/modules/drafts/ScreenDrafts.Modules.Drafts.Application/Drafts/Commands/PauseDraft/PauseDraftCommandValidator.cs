namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraft;

internal sealed class PauseDraftCommandValidator : AbstractValidator<PauseDraftCommand>
{
  public PauseDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty();
  }
}
