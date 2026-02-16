namespace ScreenDrafts.Modules.Users.Features.Admin.RemoveRoleFromUser;

internal sealed class Validator : AbstractValidator<RemoveRoleFromUserCommand>
{
  public Validator()
  {
    RuleFor(x => x.UserId)
      .NotEmpty();
    RuleFor(x => x.Role)
      .NotEmpty();
  }
}
