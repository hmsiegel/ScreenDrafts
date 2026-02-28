namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed class Validator : AbstractValidator<AssignTriviaResultsCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID must not be empty.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Results)
      .NotEmpty()
      .WithMessage("Results must not be empty.");

    RuleForEach(x => x.Results).ChildRules(r =>
    {
      r.RuleFor(x => x.ParticipantPublicId)
        .NotEmpty()
        .When(x => x.Kind == ParticipantKind.Community)
        .WithMessage("Participant public ID must not be empty.");

      r.RuleFor(x => x.Position)
        .GreaterThan(0)
        .WithMessage("Position must be greater than 0.");

      r.RuleFor(x => x.QuestionsWon)
        .GreaterThanOrEqualTo(0)
        .WithMessage("Questions won must be greater than or equal to 0.");
    });
  }
}

