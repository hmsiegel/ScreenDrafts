namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed class Validator : AbstractValidator<SubmitPredictionSetCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("DraftPartPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.SeasonPublicId)
            .NotEmpty()
            .WithMessage("SeasonPublicId is required.")
            .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.PredictionSeason))
            .WithMessage("SeasonPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.ContestantPublicId)
            .NotEmpty()
            .WithMessage("ContestantPublicId is required.")
            .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.PredictionContestant))
            .WithMessage("ContestantPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.SourceKind)
      .MustBeSmartEnumValue<SubmitPredictionSetCommand, PredictionSourceKind>()
      .WithMessage("SourceKind must be a valid value.");

    RuleFor(x => x.Entries).NotEmpty();

    RuleForEach(x => x.Entries).ChildRules(entry =>
    {
      entry.RuleFor(e => e.MediaPublicId)
        .NotEmpty()
        .WithMessage("MediaPublicId is required.")
        .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
        .WithMessage("MediaPublicId must be a valid public ID with the correct prefix.");

      entry.RuleFor(e => e.MediaTitle).NotEmpty().WithMessage("MediaTitle is required.");
    });
  }
}
