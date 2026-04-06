namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed class Validator : AbstractValidator<ApplySubDraftVetoCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("DraftPartPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartPublicId is invalid.");

    RuleFor(x => x.SubDraftPublicId)
      .NotEmpty()
      .WithMessage("SubDraftPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.SubDraft))
      .WithMessage("SubDraftPublicId is invalid.");

    RuleFor(x => x.PlayOrder).GreaterThan(0);
    RuleFor(x => x.IssuerPublicId)
      .NotEmpty()
      .WithMessage("IssuerPublicId is required.")
      .Must(id => PublicIdGuards.IsValid(id))
      .WithMessage("IssuerPublicId is invalid.");
  }
}
