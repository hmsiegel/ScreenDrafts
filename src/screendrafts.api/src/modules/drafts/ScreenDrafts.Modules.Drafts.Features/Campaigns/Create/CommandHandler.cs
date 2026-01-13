using ScreenDrafts.Common.Features.Abstractions.Services;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class CommandHandler(ICampaignsRepository campaignRepository, IPublicIdGenerator publicIdGenerator)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, string>
{
  private readonly ICampaignsRepository _campaignRepository = campaignRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(Command command, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Campaign);

    // Check for uniqueness of slug
    if (_campaignRepository.ExistsBySlug(command.Slug))
    {
      return Result.Failure<string>(CampaignErrors.DuplicateSlug);
    }

    var campaign = Campaign.Create(command.Slug, command.Name, publicId);

    _campaignRepository.Add(campaign.Value);

    return Result.Success(campaign.Value.PublicId);
  }
}


