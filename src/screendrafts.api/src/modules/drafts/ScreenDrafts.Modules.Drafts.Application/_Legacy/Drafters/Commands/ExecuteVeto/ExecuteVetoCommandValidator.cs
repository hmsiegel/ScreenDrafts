namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.ExecuteVeto;

internal sealed class ExecuteVetoCommandValidator : AbstractValidator<ExecuteVetoCommand>
{
  public ExecuteVetoCommandValidator()
  {
    RuleFor(x => x.DrafterId).NotEmpty();
    RuleFor(x => x.PickId).NotEmpty();
  }
}
