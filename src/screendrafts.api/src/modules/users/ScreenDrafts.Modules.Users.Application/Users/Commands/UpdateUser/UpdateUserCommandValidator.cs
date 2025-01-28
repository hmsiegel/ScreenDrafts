namespace ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserCommandValidator()
  {
    RuleFor(x => x.UserId)
      .NotEmpty()
      .WithMessage("User id is required.");
    RuleFor(x => x.FirstName)
      .NotEmpty()
      .WithMessage("First name is required.")
      .MaximumLength(FirstName.MaxLength)
      .WithMessage($"First name must not exceed {FirstName.MaxLength} characters.");
    RuleFor(x => x.LastName)
      .NotEmpty()
      .WithMessage("Last name is required.")
      .MaximumLength(LastName.MaxLength)
      .WithMessage($"Last name must not exceed {LastName.MaxLength} characters.");
  }
}

