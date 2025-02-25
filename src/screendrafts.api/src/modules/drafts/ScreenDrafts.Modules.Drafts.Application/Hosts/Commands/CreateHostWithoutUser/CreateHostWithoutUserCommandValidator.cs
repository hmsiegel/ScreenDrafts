namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;

internal sealed class CreateHostWithoutUserCommandValidator : AbstractValidator<CreateHostWithoutUserCommand>
{
  public CreateHostWithoutUserCommandValidator()
  {
    RuleFor(x => x.Name).NotEmpty();
  }
}
