namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed class Validator : AbstractValidator<AssignSubDraftTriviaCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with prefix '" + PublicIdPrefixes.DraftPart + "'.");

    RuleFor(x => x.SubDraftPublicId)
      .NotEmpty()
      .WithMessage("Sub-draft ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.SubDraft))
      .WithMessage("Sub-draft ID must be a valid public ID with prefix '" + PublicIdPrefixes.SubDraft + "'.");

    RuleFor(x => x.Results)
      .NotEmpty();

    RuleForEach(x => x.Results).ChildRules(x =>
    {
      x.RuleFor(r => r.Position)
        .GreaterThan(0)
        .WithMessage("Position must be greater than 0.");

      x.RuleFor(r => r.QuestionsWon)
        .GreaterThanOrEqualTo(0)
        .LessThanOrEqualTo(1)
        .WithMessage("SpeedDrafts trivia is one question. Questions won must be between 0 and 1.");
    });
  }
}
