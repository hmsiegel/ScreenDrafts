namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

internal sealed class Validator : AbstractValidator<AddRoleToUserCommand>
{
  public Validator()
  {
    RuleFor(x => x.UserId)
      .NotEmpty();
    RuleFor(x => x.Role)
      .NotEmpty();
  }
}
