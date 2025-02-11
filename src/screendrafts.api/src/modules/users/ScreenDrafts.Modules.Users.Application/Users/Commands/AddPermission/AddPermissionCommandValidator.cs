namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermission;

internal sealed class AddPermissionCommandValidator : AbstractValidator<AddPermissionCommand>
{
  public AddPermissionCommandValidator()
  {
    RuleFor(x => x.Code)
      .NotEmpty()
      .MinimumLength(2)
      .MaximumLength(100);
  }
}
