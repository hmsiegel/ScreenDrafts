namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Validator : AbstractValidator<CreateDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).MustBeSmartEnumValue<CreateDraftCommand, DraftType>();
    RuleFor(x => x.SeriesId).NotEqual(Guid.Empty);
    RuleFor(x => x.MinPosition).GreaterThan(0);
    RuleFor(x => x.MaxPosition).GreaterThanOrEqualTo(x => x.MinPosition);
  }
}

