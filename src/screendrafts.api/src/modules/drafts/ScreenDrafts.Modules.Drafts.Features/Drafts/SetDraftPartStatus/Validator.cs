namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.Request.DraftPublicId)
        .NotEmpty().WithMessage("Draft public ID must be provided.")
        .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Draft))
        .WithMessage("Draft public ID is not valid.");

    RuleFor(x => x.Request.PartIndex)
        .GreaterThanOrEqualTo(0).WithMessage("Part index must be zero or a positive integer.");

    RuleFor(x => x.Request.Action)
        .IsInEnum().WithMessage("Invalid action specified.");
  }
}
