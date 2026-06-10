namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Validator : AbstractValidator<CreateDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).MustBeSmartEnumValue<CreateDraftCommand, DraftType>();
    RuleFor(x => x.SeriesId)
      .NotEmpty()
      .WithMessage("SeriesId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Series))
      .WithMessage("SeriesId must be a valid public ID.");
  }
}
