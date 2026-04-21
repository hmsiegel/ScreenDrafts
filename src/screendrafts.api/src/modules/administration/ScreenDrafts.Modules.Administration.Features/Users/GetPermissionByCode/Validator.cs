namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed class Validator : AbstractValidator<GetPermissionByCodeQuery>
{
  public Validator()
  {
    RuleFor(x => x.Code)
      .NotEmpty()
      .WithMessage("Permission code is required.");
  }
}


