namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class PersonRequestValidator : AbstractValidator<PersonRequest>
{
  public PersonRequestValidator()
  {
    RuleFor(x => x.Name).NotEmpty();

    RuleFor(x => x.ImdbId).NotEmpty()
      .Matches(@"^nm\d{7,8}$")
      .When(x => !string.IsNullOrWhiteSpace(x.ImdbId));
  }
}

