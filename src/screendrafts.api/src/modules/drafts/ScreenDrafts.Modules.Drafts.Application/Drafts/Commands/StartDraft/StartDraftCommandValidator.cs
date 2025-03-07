namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.StartDraft;

internal sealed class StartDraftCommandValidator : AbstractValidator<StartDraftCommand>
{
  public StartDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
  }
}
