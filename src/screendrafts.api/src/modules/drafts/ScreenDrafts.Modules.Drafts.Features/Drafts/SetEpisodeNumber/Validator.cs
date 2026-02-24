namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed class Validator : AbstractValidator<SetEpisodeNumberCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("DraftId is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.Draft))
      .WithMessage("DraftId is invalid.");

    RuleFor(x => x.EpisodeNumber)
      .GreaterThan(0).WithMessage("Episode number must be greater than zero.");
  }
}
