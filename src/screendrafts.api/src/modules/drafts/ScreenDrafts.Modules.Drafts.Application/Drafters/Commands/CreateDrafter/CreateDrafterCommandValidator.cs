namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;

internal sealed class CreateDrafterCommandValidator : AbstractValidator<CreateDrafterCommand>
{
  public CreateDrafterCommandValidator()
  {
    RuleFor(x => x.Name).NotEmpty();
  }
}
