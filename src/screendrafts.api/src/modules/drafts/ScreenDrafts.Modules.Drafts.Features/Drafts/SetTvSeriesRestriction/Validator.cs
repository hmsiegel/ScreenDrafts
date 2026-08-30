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

    // The domain type allows TvSeriesTitle to be set independently of
    // TvSeriesTmdbId (Draft.SetTvSeriesRestriction doesn't enforce the
    // pairing itself), but every real caller is a search-and-pick UI that
    // always has both together — so this is where the pairing actually gets
    // enforced, rather than in the domain.
    RuleFor(x => x.TvSeriesTitle)
      .NotEmpty()
      .WithMessage("TvSeriesTitle is required when TvSeriesTmdbId is set.")
      .When(x => x.TvSeriesTmdbId.HasValue);
  }
}
