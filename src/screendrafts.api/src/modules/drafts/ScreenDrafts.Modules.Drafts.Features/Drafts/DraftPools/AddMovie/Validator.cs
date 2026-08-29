namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed class Validator : AbstractValidator<AddMovieToDraftPoolRequest>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.DraftPool))
      .WithMessage("PublicId is invalid.");
    RuleFor(x => x.TmdbId).GreaterThan(0).WithMessage("TmdbId must be greater than 0.");

    RuleFor(x => x.TvSeriesTmdbId)
      .NotNull()
      .WithMessage("TvSeriesTmdbId is required for TV episodes.")
      .When(x => x.MediaType == MediaType.TvEpisode);

    RuleFor(x => x.SeasonNumber)
      .NotNull()
      .WithMessage("SeasonNumber is required for TV episodes.")
      .When(x => x.MediaType == MediaType.TvEpisode);

    RuleFor(x => x.EpisodeNumber)
      .NotNull()
      .WithMessage("EpisodeNumber is required for TV episodes.")
      .When(x => x.MediaType == MediaType.TvEpisode);
  }
}
