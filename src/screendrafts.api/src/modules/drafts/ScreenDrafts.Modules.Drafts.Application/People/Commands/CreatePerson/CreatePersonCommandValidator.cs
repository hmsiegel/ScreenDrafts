namespace ScreenDrafts.Modules.Drafts.Application.People.Commands.CreatePerson;

internal sealed class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
  public CreatePersonCommandValidator()
  {
    RuleFor(x => x.FirstName)
        .NotEmpty()
        .MaximumLength(100)
        .WithMessage("First name is required and cannot exceed 100 characters.");
    RuleFor(x => x.LastName)
        .NotEmpty()
        .MaximumLength(100)
        .WithMessage("Last name is required and cannot exceed 100 characters.");
  }
}
