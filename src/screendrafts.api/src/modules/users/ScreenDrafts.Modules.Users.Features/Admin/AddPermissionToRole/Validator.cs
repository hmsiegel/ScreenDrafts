namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermissionToRole;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.Role)
      .NotEmpty()
      .WithMessage("Role is required.");
    RuleFor(x => x.Permission)
      .NotEmpty()
      .WithMessage("Permission is required.");
  }
}
