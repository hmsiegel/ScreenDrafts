namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class Validator : AbstractValidator<AddMediaCommand>
{
  public Validator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .WithMessage("Title is required")
      .MaximumLength(255)
      .WithMessage("Title must not exceed 255 characters");

    // ImdbId ir required for movies and music videos, optional for tv shows,
    // TV episodes and video games which may not have an IMDb Id on TMDb.
    RuleFor(x => x.ImdbId)
      .NotEmpty()
      .WithMessage("IMDb ID is required")
      .Matches(@"^tt\d{7,8}$")
      .WithMessage("IMDb ID must be in the format 'tt' followed by 7 or 8 digits")
      .When(x => x.MediaType == MediaType.Movie || x.MediaType == MediaType.MusicVideo);

    RuleFor(x => x.ImdbId)
      .Matches(@"^tt\d{7,8}$")
      .WithMessage("IMDb ID must be in the format 'tt' followed by 7 or 8 digits")
      .When(x => !string.IsNullOrWhiteSpace(x.ImdbId)
        && x.MediaType != MediaType.Movie 
        && x.MediaType != MediaType.MusicVideo);

    RuleFor(x => x.Year)
      .NotEmpty()
      .WithMessage("Year is required")
      .Matches(@"^\d{4}$")
      .WithMessage("Year must be a 4-digit number.")
      .InclusiveBetween("1900", DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture))
      .WithMessage("Year must be between 1900 and the current year");

    RuleFor(x => x.Plot)
      .NotEmpty()
      .WithMessage("Plot is required")
      .When(x => x.MediaType != MediaType.MusicVideo);

    RuleFor(x => x.Image)
      .NotEmpty()
      .WithMessage("Image is required")
      .When(x => x.MediaType != MediaType.MusicVideo);

    RuleFor(x => x.ReleaseDate)
      .Must(BeAValidDate)
      .WithMessage("Release date must be in the format 'yyyy-MM-dd'");

    RuleFor(x => x.YouTubeTrailerUrl)
      .Must(BeAValidTrailerUrl)
      .WithMessage("YouTube trailer URL must be a valid URL");

    // Genres are optional for video games and TV episodes
    RuleFor(x => x.Genres)
      .NotEmpty()
      .When(x => x.MediaType != MediaType.VideoGame && x.MediaType != MediaType.TvEpisode);

    RuleForEach(x => x.Directors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Actors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Writers).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Producers).SetValidator(new PersonRequestValidator());
  }

  private static bool BeAValidDate(string? date)
  {
    if (string.IsNullOrWhiteSpace(date))
    {
      return true;
    }

    return DateTime.TryParseExact(
      date,
      "yyyy-MM-dd",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out _);
  }

  private static bool BeAValidTrailerUrl(Uri? url)
  {
    return url is null
      || url.Host == "www.youtube.com"
      || url.Host == "youtu.be";
  }
}

