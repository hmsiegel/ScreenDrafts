namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Campaign))
      .WithMessage("PublicId is required.");
  }
}
