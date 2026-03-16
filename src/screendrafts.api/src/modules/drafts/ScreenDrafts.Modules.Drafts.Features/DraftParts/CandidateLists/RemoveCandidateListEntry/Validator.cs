namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed class Validator : AbstractValidator<RemoveCandidateListEntryCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("DraftPartId is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartId is invalid.");

    RuleFor(x => x.TmdbId)
      .GreaterThan(0)
      .WithMessage("TmdbId is invalid.");
  }
}
