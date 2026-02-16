namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class Validator : AbstractValidator<CreateDrafterCommand>
{
  public Validator()
  {
    RuleFor(x => x.PersonId)
      .NotEmpty().WithMessage("PersonId is required.")
      .Must(personId => PublicIdGuards.IsValidWithPrefix(personId, PublicIdPrefixes.Person))
      .WithMessage("Public PersonId is invalid.");
  }
}

