namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed class Validator : AbstractValidator<SendPasswordResetCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.User))
      .WithMessage("Invalid PublicId format.");
  }
}
