namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed class Validator : AbstractValidator<AddMovieCommand>
{
  public Validator()
  {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("The draft ID must be provided.");
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("The Public ID must be provided.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
      .WithMessage("The public Id must be of the correct format.");
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("The movie title must be provided.")
      .MaximumLength(500).WithMessage("The movie title must not exceed 500 characters.");

    RuleFor(x => x)
      .Must(x => x.ImdbId is not null || x.TmdbId is not null || x.IgdbId is not null)
      .WithMessage("At least one of ImdbId, TmdbId or IgdbId must be provided.");
  }
}

