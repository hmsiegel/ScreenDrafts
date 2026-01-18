namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermission;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.Code)
      .NotEmpty()
      .MinimumLength(2)
      .MaximumLength(100);
  }
}
