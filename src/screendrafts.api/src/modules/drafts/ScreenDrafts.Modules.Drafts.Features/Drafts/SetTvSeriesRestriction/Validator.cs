namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed class Validator : AbstractValidator<SetTvSeriesRestrictionRequest>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Draft))
      .WithMessage("PublicId is invalid.");

    RuleFor(x => x.TvSeriesTmdbId)
      .GreaterThan(0)
      .When(x => x.TvSeriesTmdbId.HasValue)
      .WithMessage("TvSeriesTmdbId must be greater than 0.");
  }
}
