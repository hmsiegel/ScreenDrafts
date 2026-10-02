namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;

// ── Validator ─────────────────────────────────────────────────────────────

internal sealed class Validator : AbstractValidator<UpdateSpotlightCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Spotlight))
      .WithMessage("Spotlight public ID is invalid.");

    RuleFor(x => x.SpotlightDescription)
      .NotEmpty()
      .MaximumLength(1000)
      .WithMessage("Spotlight description is required and must not exceed 1000 characters.");

    RuleFor(x => x.SpotifyUrl)
      .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
      .When(x => !string.IsNullOrWhiteSpace(x.SpotifyUrl))
      .WithMessage("Spotify URL must be a valid absolute URL.");
  }
}
