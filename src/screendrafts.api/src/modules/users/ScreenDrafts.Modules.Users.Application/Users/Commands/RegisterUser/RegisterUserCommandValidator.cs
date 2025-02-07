namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
  public RegisterUserCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();
    RuleFor(x => x.Password)
      .MinimumLength(6);
    RuleFor(x => x.FirstName)
      .NotEmpty();
    RuleFor(x => x.LastName)
      .NotEmpty();
  }
}
