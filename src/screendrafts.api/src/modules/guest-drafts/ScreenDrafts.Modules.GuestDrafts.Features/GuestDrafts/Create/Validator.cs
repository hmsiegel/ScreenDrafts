namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed class Validator : AbstractValidator<CreateGuestDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.OwnerUserPublicId)
      .NotEmpty()
      .WithMessage("OwnerUserPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("OwnerUserPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Title).NotEmpty().MaximumLength(GuestDraft.TitleMaxLength);

    RuleFor(x => x.Type).NotEmpty();
  }
}
