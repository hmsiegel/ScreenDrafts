
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed class Validator : AbstractValidator<Query>
{
  public Validator()
  {
    RuleFor(x => x.PersonIds)
    .NotEmpty().WithMessage("The list of person IDs cannot be empty.")
    .Must(ids => ids.Any(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person)))
    .WithMessage("One or more person IDs are not valid.");
  }
}
