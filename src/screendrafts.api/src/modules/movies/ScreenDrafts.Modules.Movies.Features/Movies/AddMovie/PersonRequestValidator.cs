namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed class PersonRequestValidator : AbstractValidator<PersonRequest>
{
  public PersonRequestValidator()
  {
    RuleFor(x => x.Name).NotEmpty();
    RuleFor(x => x.ImdbId).NotEmpty();
  }
}

