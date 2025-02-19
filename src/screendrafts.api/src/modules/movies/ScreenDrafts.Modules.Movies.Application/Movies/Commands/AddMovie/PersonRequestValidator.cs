namespace ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;

internal sealed class PersonRequestValidator : AbstractValidator<PersonRequest>
{
  public PersonRequestValidator()
  {
    RuleFor(x => x.Name).NotEmpty();
    RuleFor(x => x.ImdbId).NotEmpty();
  }
}

