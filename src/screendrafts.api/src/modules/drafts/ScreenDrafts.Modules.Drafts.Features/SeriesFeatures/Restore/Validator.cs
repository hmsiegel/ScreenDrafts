namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Restore;

internal sealed class Validator : AbstractValidator<RestoreSeriesCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId must not be empty.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Series))
      .WithMessage("PublicId must be a valid Series public ID.");
  }
}
