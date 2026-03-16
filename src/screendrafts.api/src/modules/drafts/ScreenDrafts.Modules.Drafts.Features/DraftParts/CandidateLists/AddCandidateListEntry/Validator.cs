namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed class Validator : AbstractValidator<AddCandidateEntryCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.TmdbId)
      .GreaterThan(0)
      .WithMessage("TMDB ID must be a positive integer.");

    RuleFor(x => x.Notes)
      .MaximumLength(1000)
      .WithMessage("Notes cannot exceed 1000 characters.")
      .When(x => !string.IsNullOrEmpty(x.Notes));

    RuleFor(x => x.AddedByPublicId)
      .NotEmpty()
      .WithMessage("Added by public ID is required.")
      .Must(id => PublicIdGuards.IsValid(id))
      .WithMessage("Added by public ID must be a valid public ID with the correct prefix.");
  }
}
