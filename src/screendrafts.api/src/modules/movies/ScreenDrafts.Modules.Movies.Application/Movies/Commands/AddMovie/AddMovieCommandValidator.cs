namespace ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;

internal sealed class AddMovieCommandValidator : AbstractValidator<AddMovieCommand>
{
  public AddMovieCommandValidator()
  {
    RuleFor(x => x.ImdbId).NotEmpty();
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.Year).NotEmpty();
    RuleFor(x => x.Plot).NotEmpty();
    RuleFor(x => x.Image).NotEmpty();
    RuleFor(x => x.ReleaseDate).NotEmpty();
    RuleFor(x => x.YouTubeTrailerUrl).NotEmpty();
    RuleFor(x => x.Genres).NotEmpty();
    RuleForEach(x => x.Directors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Actors).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Writers).SetValidator(new PersonRequestValidator());
    RuleForEach(x => x.Producers).SetValidator(new PersonRequestValidator());
  }
}

