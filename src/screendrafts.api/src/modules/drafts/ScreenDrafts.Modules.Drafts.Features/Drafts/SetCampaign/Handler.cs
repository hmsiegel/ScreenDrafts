namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class Handler(IDraftsRepository draftRepository, ICampaignsRepository campaignRepository) : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IDraftsRepository _draftsRepository = draftRepository;
  private readonly ICampaignsRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var campaign = await _campaignRepository.GetByPublicIdAsync(request.CampaignId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(DraftErrors.CampaignNotFound(request.CampaignId));
    }

    draft.SetCampaign(campaign);
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}
