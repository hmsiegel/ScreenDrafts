using System.Globalization;

namespace ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;

internal sealed class AddMovieCommandValidator : AbstractValidator<AddMovieCommand>
{
  public AddMovieCommandValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .WithMessage("Title is required")
      .MaximumLength(255)
      .WithMessage("Title must not exceed 255 characters");

    RuleFor(x => x.ImdbId)
      .NotEmpty()
      .WithMessage("IMDb ID is required")
      .Matches(@"^tt\d{7,8}$")
      .WithMessage("IMDb ID must be in the format 'tt' followed by 7 or 8 digits");

    RuleFor(x => x.Year)
      .NotEmpty()
      .WithMessage("Year is required")
      .Matches(@"^\d{4}$")
      .WithMessage("Year must be a 4-digit number.")
      .InclusiveBetween("1900", DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture))
      .WithMessage("Year must be between 1900 and the current year");

    RuleFor(x => x.Plot)
      .NotEmpty()
      .WithMessage("Plot is required");

    RuleFor(x => x.Image)
      .NotEmpty()
      .WithMessage("Image is required")
      .Must(BeAValidUrl)
      .WithMessage("Image must be a valid URL");

    RuleFor(x => x.ReleaseDate)
      .Must(BeAValidDate)
      .WithMessage("Release date must be in the format 'yyyy-MM-dd'");

    RuleFor(x => x.YouTubeTrailerUrl)
      .Must(BeAValidTrailerUrl)
      .WithMessage("YouTube trailer URL must be a valid URL");

    RuleFor(x => x.Genres).NotEmpty();
    RuleForEach(x => x.Directors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Actors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Writers).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Producers).SetValidator(new PersonRequestValidator());
  }

  private static bool BeAValidUrl(string url)
  {
    return Uri.TryCreate(url, UriKind.Absolute, out _);
  }

  private static bool BeAValidDate(string? date)
  {
    return DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
  }

  private static bool BeAValidTrailerUrl(Uri? url)
  {
    return url is null || url.Host == "www.imdb.com";
  }
}

