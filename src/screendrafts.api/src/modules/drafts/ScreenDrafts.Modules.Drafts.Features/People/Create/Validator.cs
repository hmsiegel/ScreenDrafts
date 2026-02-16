namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class Validator : AbstractValidator<CreatePersonCommand>
{
  public Validator()
  {
    RuleFor(x => x)
      .Must(x => x.UserId.HasValue ||
      (!string.IsNullOrWhiteSpace(x.FirstName) && !string.IsNullOrWhiteSpace(x.LastName)))
      .WithMessage("Either UserId or both First and Last Name are required.");

    RuleFor(x => x.UserId)
      .NotEqual(Guid.Empty)
      .When(x => x.UserId.HasValue)
      .WithMessage("UserId cannot be an empty GUID if it is being used.");

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(100)
      .WithMessage("First Name must be between 1 and 100 characters.");

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(100)
      .WithMessage("Last Name must be between 1 and 100 characters.");
  }
}

