namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class CreateCampaignCommandHandler(ICampaignRepository campaignRepository, IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreateCampaignCommand, string>
{
  private readonly ICampaignRepository _campaignRepository = campaignRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreateCampaignCommand CreateCampaignCommand, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Campaign);

    // Check for uniqueness of slug
    if (_campaignRepository.ExistsBySlug(CreateCampaignCommand.Slug))
    {
      return Result.Failure<string>(CampaignErrors.DuplicateSlug);
    }

    var campaign = Campaign.Create(CreateCampaignCommand.Slug, CreateCampaignCommand.Name, publicId);

    _campaignRepository.Add(campaign.Value);

    return Result.Success(campaign.Value.PublicId);
  }
}




