namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed class Validator : AbstractValidator<StartZoomSessionCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("DraftPartPublicId cannot be emtpy.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartPublicId must be in the correct format.");

    RuleFor(x => x.HostPublicId)
      .NotEmpty()
      .WithMessage("HostPublicId cannot be emtpy.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.Host))
      .WithMessage("HostPublicId must be in the correct format.");
  }
}
