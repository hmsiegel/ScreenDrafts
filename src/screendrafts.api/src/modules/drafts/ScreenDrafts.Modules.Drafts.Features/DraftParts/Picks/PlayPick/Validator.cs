namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed class Validator : AbstractValidator<PlayPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID.");

    RuleFor(x => x.Position).GreaterThanOrEqualTo(0);

    RuleFor(x => x.PlayOrder).GreaterThanOrEqualTo(0);

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.")
      .Must(id => PublicIdGuards.IsValid(id!))
      .WithMessage("Participant ID must be a valid public ID.")
      .When(x => x.ParticipantKind != ParticipantKind.Community.Value);

    // Either MoviePublicId or TmdbId must be provided.
    // TmdbId is used for community picks where the public ID is unknown.
    RuleFor(x => x)
      .Must(x => !string.IsNullOrWhiteSpace(x.MoviePublicId) || x.TmdbId.HasValue)
      .WithMessage("Either Movie public ID or TMDb ID must be provided.")
      .WithName("MovieIdentifier");

    RuleFor(x => x.MoviePublicId)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
      .WithMessage("Movie public ID must be a valid public ID.")
      .When(x => !string.IsNullOrWhiteSpace(x.MoviePublicId));

    RuleFor(x => x.TmdbId)
      .GreaterThan(0)
      .WithMessage("TMDb ID must be greater than zero.")
      .When(x => x.TmdbId.HasValue);
  }
}
