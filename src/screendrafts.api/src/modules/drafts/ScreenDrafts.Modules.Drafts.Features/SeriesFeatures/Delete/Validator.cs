namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Delete;

internal sealed class Validator : AbstractValidator<DeleteSeriesCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("Series public id must be provided.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Series))
      .WithMessage("Series public id is not valid.");
  }
}
