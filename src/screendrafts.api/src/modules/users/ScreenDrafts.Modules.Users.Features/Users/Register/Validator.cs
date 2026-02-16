namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class Validator : AbstractValidator<RegisterUserCommand>
{
  public Validator()
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
