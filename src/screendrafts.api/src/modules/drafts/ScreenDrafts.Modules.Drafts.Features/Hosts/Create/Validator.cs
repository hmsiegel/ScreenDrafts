namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class Validator : AbstractValidator<CreateHostCommand>
{
  public Validator()
  {
    RuleFor(x => x.PersonPublicId)
      .NotEmpty().WithMessage("Person public ID cannot be empty.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      .WithMessage("Person public ID is not valid.");
  }
}

