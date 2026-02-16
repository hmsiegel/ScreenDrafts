namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveDrafterFromDraftPart;

internal sealed class RemoveDrafterFromDraftPartCommandValidator : AbstractValidator<RemoveDrafterFromDraftPartCommand>
{
  public RemoveDrafterFromDraftPartCommandValidator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty();
    RuleFor(x => x.DrafterId).NotEmpty();
  }
}
