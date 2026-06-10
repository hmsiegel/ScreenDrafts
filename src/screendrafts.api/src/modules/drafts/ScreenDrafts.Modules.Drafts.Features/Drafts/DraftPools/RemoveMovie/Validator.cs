namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.RemoveMovie;

// ── Validator ─────────────────────────────────────────────────────────────────

internal sealed class Validator
  : AbstractValidator<RemoveMovieFromDraftPoolCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId).NotEmpty();
    RuleFor(x => x.TmdbId).GreaterThan(0);
  }
}
