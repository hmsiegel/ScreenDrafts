namespace ScreenDrafts.Modules.Users.Features.Users.GetUser;

internal sealed class Validator : AbstractValidator<GetUserQuery>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.User))
      .WithMessage("The public ID is not valid.");
  }
}
