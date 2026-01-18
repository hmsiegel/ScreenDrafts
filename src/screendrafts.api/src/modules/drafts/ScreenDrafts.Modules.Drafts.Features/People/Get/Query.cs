namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed record Query(string PublicId) : IQuery<PersonResponse>;

internal sealed class Validator : AbstractValidator<Query>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      .WithMessage("The provided public ID is not valid.");
  }
}
