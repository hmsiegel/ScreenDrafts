namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Validator : AbstractValidator<DeleteCampaignCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Campaign))
      .WithMessage("PublicId is required.");
  }
}

