namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class RestoreCampaignCommandHandler(ICampaignRepository campaignsRepository)
  : ICommandHandler<RestoreCampaignCommand>
{
  private readonly ICampaignRepository _campaignsRepository = campaignsRepository;

  public async Task<Result> Handle(RestoreCampaignCommand RestoreCampaignRequest, CancellationToken cancellationToken)
  {
    var campaign = await _campaignsRepository.GetByPublicIdAsync(RestoreCampaignRequest.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(RestoreCampaignRequest.PublicId));
    }

    var restoreResult = campaign.Restore();

    if (restoreResult.IsFailure)
    {
      return Result.Failure(restoreResult.Errors);
    }

    _campaignsRepository.Update(campaign);

    return Result.Success();
  }
}



