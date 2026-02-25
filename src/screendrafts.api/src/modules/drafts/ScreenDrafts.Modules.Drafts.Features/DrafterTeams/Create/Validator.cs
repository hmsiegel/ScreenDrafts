namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed class Validator : AbstractValidator<CreateDrafterTeamCommand>
{
  public Validator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100)
      .MinimumLength(3)
      .WithMessage("Name is required and must be between 3 and 100 characters long.");
  }
}
