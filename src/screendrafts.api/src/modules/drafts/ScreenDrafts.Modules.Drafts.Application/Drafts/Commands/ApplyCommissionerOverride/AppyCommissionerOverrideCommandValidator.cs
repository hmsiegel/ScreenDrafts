
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ApplyCommissionerOverride;

internal sealed class AppyCommissionerOverrideCommandValidator : AbstractValidator<ApplyCommissionerOverrideCommand>
{
  public AppyCommissionerOverrideCommandValidator()
  {
    RuleFor(x => x.PickId)
      .NotEmpty();
  }
}
