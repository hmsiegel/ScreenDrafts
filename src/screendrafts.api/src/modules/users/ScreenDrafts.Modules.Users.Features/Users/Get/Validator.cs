namespace ScreenDrafts.Modules.Users.Features.Users.Get;

internal sealed class Validator : AbstractValidator<Query>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.User))
      .WithMessage("The public ID is not valid.");
  }
}
