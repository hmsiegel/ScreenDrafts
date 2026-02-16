namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class SetCampaignDraftCommandHandler(IDraftRepository draftRepository, ICampaignRepository campaignRepository) : ICommandHandler<SetCampaignDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftRepository;
  private readonly ICampaignRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(SetCampaignDraftCommand SetCampaignDraftRequest, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(SetCampaignDraftRequest.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(SetCampaignDraftRequest.DraftId));
    }

    var campaign = await _campaignRepository.GetByPublicIdAsync(SetCampaignDraftRequest.CampaignId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.CampaignNotFound(SetCampaignDraftRequest.CampaignId));
    }

    draft.SetCampaign(campaign);
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}



