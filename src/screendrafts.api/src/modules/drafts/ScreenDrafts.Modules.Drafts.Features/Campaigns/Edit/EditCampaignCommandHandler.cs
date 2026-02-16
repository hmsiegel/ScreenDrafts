namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class EditCampaignCommandHandler(ICampaignRepository campaignRepository)
  : ICommandHandler<EditCampaignCommand>
{
  private readonly ICampaignRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(EditCampaignCommand EditCampaignRequest, CancellationToken cancellationToken)
  {
    var campaign = await _campaignRepository.GetByPublicIdAsync(EditCampaignRequest.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(EditCampaignRequest.PublicId));
    }

    if (EditCampaignRequest.Name is not null)
    {
      var renameResult = campaign.Rename(EditCampaignRequest.Name);
      if (renameResult.IsFailure)
      {
        return renameResult;
      }
    }

    if (EditCampaignRequest.Slug is not null)
    {
      var changeSlugResult = campaign.ChangeSlug(EditCampaignRequest.Slug);
      if (changeSlugResult.IsFailure)
      {
        return changeSlugResult;
      }
    }

    _campaignRepository.Update(campaign);

    return Result.Success();
  }
}



