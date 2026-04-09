namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed class Validator
  : AbstractValidator<CreatePredictionContestantCommand>
{
  public Validator()
  {
    RuleFor(x => x.PersonPublicId)
      .NotEmpty()
      .WithMessage("Person public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Person))
      .WithMessage("Invalid person public ID format.");
  }
}
