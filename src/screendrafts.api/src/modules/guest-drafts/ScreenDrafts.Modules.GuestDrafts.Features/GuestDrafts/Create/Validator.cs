namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed class Validator : AbstractValidator<CreateGuestDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.OwnerUserPublicId)
      .NotEmpty();

    RuleFor(x => x.Title)
      .NotEmpty()
      .MaximumLength(GuestDraft.TitleMaxLength);

    RuleFor(x => x.Type)
      .NotEmpty();
  }
}
