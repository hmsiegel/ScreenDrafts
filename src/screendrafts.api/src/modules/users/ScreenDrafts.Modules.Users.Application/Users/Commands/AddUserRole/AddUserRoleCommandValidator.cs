namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddUserRole;

internal sealed class AddUserRoleCommandValidator : AbstractValidator<AddUserRoleCommand>
{
  public AddUserRoleCommandValidator()
  {
    RuleFor(x => x.UserId)
      .NotEmpty();
    RuleFor(x => x.Role)
      .NotEmpty();
  }
}
