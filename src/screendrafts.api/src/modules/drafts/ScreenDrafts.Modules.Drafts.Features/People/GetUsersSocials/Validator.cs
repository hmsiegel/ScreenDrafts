using ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed class Validator : AbstractValidator<GetUsersSocialsQuery>
{
  public Validator()
  {
    RuleFor(x => x.PersonIds)
      .NotEmpty()
      .WithMessage("The list of person IDs cannot be empty.")
      .Must(ids =>
        ids.Any(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      )
      .WithMessage("One or more person IDs are not valid.");
  }
}
