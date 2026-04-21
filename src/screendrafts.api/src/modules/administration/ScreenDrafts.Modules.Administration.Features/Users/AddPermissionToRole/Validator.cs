namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed class Validator : AbstractValidator<AddPermissionToRoleRequest>
{
  public Validator()
  {
    RuleFor(x => x.PermissionCode)
      .NotEmpty()
      .MaximumLength(100);
    RuleFor(x => x.RoleName)
      .NotEmpty()
      .MaximumLength(100);
  }
}
