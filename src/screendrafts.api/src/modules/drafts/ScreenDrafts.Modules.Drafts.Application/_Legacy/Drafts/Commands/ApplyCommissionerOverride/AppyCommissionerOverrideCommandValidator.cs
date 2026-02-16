namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.ApplyCommissionerOverride;

internal sealed class AppyCommissionerOverrideCommandValidator : AbstractValidator<ApplyCommissionerOverrideCommand>
{
  public AppyCommissionerOverrideCommandValidator()
  {
    RuleFor(x => x.PickId)
      .NotEmpty();
  }
}
