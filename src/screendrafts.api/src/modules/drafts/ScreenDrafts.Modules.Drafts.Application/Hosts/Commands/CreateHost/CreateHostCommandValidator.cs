namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;

internal sealed class CreateHostCommandValidator : AbstractValidator<CreateHostCommand>
{
  public CreateHostCommandValidator()
  {
    RuleFor(x => x.PersonId)
      .NotEmpty();
  }
}
