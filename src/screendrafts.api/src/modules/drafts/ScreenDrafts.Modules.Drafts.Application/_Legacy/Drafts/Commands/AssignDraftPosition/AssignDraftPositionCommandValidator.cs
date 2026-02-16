namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AssignDraftPosition;

internal sealed class AssignDraftPositionCommandValidator : AbstractValidator<AssignDraftPositionCommand>
{
  public AssignDraftPositionCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.DrafterId).NotEmpty();
    RuleFor(x => x.PositionId).NotEmpty();
  }
}
