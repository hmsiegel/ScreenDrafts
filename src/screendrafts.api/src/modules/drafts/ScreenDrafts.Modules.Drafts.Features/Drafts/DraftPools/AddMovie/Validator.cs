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
    RuleFor(x => x.TmdbId)
      .GreaterThan(0)
      .WithMessage("TmdbId must be greater than 0.");
  }
}
