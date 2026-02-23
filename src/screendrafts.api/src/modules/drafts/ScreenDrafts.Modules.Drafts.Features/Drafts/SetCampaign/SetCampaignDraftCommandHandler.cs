namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class SetCampaignDraftCommandHandler(IDraftRepository draftRepository, ICampaignRepository campaignRepository) : ICommandHandler<SetCampaignDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftRepository;
  private readonly ICampaignRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(SetCampaignDraftCommand command, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(command.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(command.DraftId));
    }

    var campaign = await _campaignRepository.GetByPublicIdAsync(command.CampaignId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.CampaignNotFound(command.CampaignId));
    }

    draft.SetCampaign(campaign);
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}



