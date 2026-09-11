namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.UpdateDraft;

internal sealed class Validator : AbstractValidator<UpdateDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("GuestDraftPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("GuestDraftPublicId must be a valid public ID with the correct prefix.");
  }
}
