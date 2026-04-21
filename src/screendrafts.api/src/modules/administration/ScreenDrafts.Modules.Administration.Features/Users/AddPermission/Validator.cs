namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed class Validator : AbstractValidator<AddPermissionRequest>
{
  public Validator()
  {
    RuleFor(x => x.Code)
      .NotEmpty()
      .WithMessage("Permission must not be empty.")
      .Matches("\\w+:\\w+");
  }
}
