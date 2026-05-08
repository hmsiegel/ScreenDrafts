namespace ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;

internal sealed class Validator : AbstractValidator<CreateSpotlightCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPublicId)
      .NotEmpty()
      .WithMessage("Draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("Draft public ID is invalid.");

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
