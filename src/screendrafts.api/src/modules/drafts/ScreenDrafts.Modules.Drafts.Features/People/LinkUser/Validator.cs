
namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed class Validator : AbstractValidator<LinkUserPersonCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("PublicId is required.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      .WithMessage("PublicId is not valid or does not have the correct prefix.");

    RuleFor(x => x.UserId)
      .NotEmpty().WithMessage("UserId is required.");
  }
}

