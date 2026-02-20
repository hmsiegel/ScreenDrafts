namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class Validator : AbstractValidator<RegisterUserCommand>
{
  public Validator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();
    
    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(8)
      .WithErrorCode("User.Password.TooShort")
      .WithMessage("Password must be at least 8 characters long.");
    
    RuleFor(x => x.FirstName)
      .NotEmpty();
    
    RuleFor(x => x.LastName)
      .NotEmpty();
  }
}
