namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveHostFromDraft;

internal sealed class RemoveHostFromDraftCommandValidator : AbstractValidator<RemoveHostFromDraftCommand>
{
  public RemoveHostFromDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.HostId).NotEmpty();
  }
}
