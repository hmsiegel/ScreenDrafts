namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;

internal sealed class CreateDrafterCommandValidator : AbstractValidator<CreateDrafterCommand>
{
  public CreateDrafterCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);
    RuleFor(x => x.UserId)
      .NotEmpty();
  }
}
