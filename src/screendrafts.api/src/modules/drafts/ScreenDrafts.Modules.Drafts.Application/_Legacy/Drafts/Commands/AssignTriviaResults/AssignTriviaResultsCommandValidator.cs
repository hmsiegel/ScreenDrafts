namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AssignTriviaResults;

internal sealed class AssignTriviaResultsCommandValidator : AbstractValidator<AssignTriviaResultsCommand>
{
  public AssignTriviaResultsCommandValidator()
  {
    RuleFor(x => x.DrafterId)
      .NotEmpty();
    RuleFor(x => x.Position)
      .GreaterThan(0);
  }
}
