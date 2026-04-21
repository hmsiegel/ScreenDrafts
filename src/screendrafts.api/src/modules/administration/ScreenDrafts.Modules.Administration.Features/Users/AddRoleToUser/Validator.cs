namespace ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;

internal sealed class Validator : AbstractValidator<AddRoleToUserCommand>
{
  public Validator()
  {
    RuleFor(x => x.UserPublicId)
      .NotEmpty()
      .WithMessage("User public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("User public ID is invalid.");

    RuleFor(x => x.RoleName)
      .NotEmpty()
      .WithMessage("Role name is required.");
  }
}
