namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateProfile;

internal sealed class Validator : AbstractValidator<UpdateProfileCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Person))
      .WithMessage("PublicId is not valid.");
    RuleFor(x => x.DisplayName).MaximumLength(100).WithMessage("DisplayName is too long.");
    RuleFor(x => x.Biography).MaximumLength(1000).WithMessage("Biography is too long.");
    RuleFor(x => x.Location).MaximumLength(100).WithMessage("Location is too long.");
  }
}
