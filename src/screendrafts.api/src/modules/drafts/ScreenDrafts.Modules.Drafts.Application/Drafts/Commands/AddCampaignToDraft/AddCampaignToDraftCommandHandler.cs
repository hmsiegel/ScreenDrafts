namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCampaignToDraft;

internal sealed class AddCampaignToDraftCommandHandler(
  IDraftsRepository draftsRepository,
  ICampaignRepository campaignRepository)
  : ICommandHandler<AddCampaignToDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly ICampaignRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(AddCampaignToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(DraftErrors.CampaignNotFound(request.CampaignId));
    }

    draft.AddCampaign(campaign);

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}

