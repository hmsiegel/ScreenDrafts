namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.User))
      .WithMessage("PublicId is required and must be a valid user public id.");

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(FirstName.MaxLength)
      .WithMessage($"First name is required and must not exceed {FirstName.MaxLength} characters.");

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(LastName.MaxLength)
      .WithMessage($"Last name is required and must not exceed {LastName.MaxLength} characters.");
  }
}
