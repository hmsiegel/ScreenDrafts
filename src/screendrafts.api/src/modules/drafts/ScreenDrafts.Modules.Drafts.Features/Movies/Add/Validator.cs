namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed class Validator : AbstractValidator<AddMovieCommand>
{
  public Validator()
  {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("The draft ID must be provided.");
    RuleFor(x => x.ImdbId)
      .NotEmpty().WithMessage("The IMDb ID must be provided.")
      .MaximumLength(20).WithMessage("The IMDb ID must not exceed 20 characters.");
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("The movie title must be provided.")
      .MaximumLength(500).WithMessage("The movie title must not exceed 500 characters.");
  }
}

