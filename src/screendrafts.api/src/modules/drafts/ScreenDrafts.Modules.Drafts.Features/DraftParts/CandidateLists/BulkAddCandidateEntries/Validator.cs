namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed class Validator : AbstractValidator<BulkAddCandidateEntriesCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("DraftPartId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.CsvStream)
      .NotEmpty()
      .WithMessage("CSV file is required.");

    RuleFor(x => x.AddedByPublicId)
      .NotEmpty()
      .WithMessage("AddedByPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("AddedByPublicId must be a valid public ID with the correct prefix.");
  }
}
