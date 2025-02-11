namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RemoveUserRole;

internal sealed class RemoveUserRoleCommandValidator : AbstractValidator<RemoveUserRoleCommand>
{
  public RemoveUserRoleCommandValidator()
  {
    RuleFor(x => x.UserId)
      .NotEmpty();
    RuleFor(x => x.Role)
      .NotEmpty();
  }
}
