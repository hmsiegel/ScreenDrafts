namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class DeleteCampaignCommandHandler(ICampaignRepository campaignsRepository)
  : ICommandHandler<DeleteCampaignCommand>
{
  private readonly ICampaignRepository _campaignsRepository = campaignsRepository;

  public async Task<Result> Handle(DeleteCampaignCommand DeleteCampaignRequest, CancellationToken cancellationToken)
  {
    var campaign = await _campaignsRepository.GetByPublicIdAsync(DeleteCampaignRequest.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(DeleteCampaignRequest.PublicId));
    }

    if (campaign.IsDeleted)
    {
      return Result.Success();
    }

    var deleteResult = campaign.SoftDelete();

    if (deleteResult.IsFailure)
    {
      return Result.Failure(CampaignErrors.DeletionFailed(DeleteCampaignRequest.PublicId));
    }

    return Result.Success();
  }
}



