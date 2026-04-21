namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;

internal sealed class Validator : AbstractValidator<RemoveRoleFromUserCommand>
{
  public Validator()
  {
    RuleFor(x => x.UserPublicId)
      .NotEmpty()
      .WithMessage("User public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id , PublicIdPrefixes.User))
      .WithMessage("Invalid user public ID.");

    RuleFor(x => x.RoleName)
      .NotNull()
      .WithMessage("Role name is required.");
  }
}


