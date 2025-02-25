namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraft;

internal sealed class CompleteDraftCommandValidator : AbstractValidator<CompleteDraftCommand>
{
  public CompleteDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
  }
}
