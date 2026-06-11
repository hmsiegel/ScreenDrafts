namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed class Validator : AbstractValidator<ActivateSpotlightCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Spotlight))
      .WithMessage("PublicId is not valid.");
  }
}
