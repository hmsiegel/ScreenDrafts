namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class ClearCampaignDraftCommandHandler(
  IDraftRepository draftsRepository)
  : ICommandHandler<ClearCampaignDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(ClearCampaignDraftCommand ClearCampaignDraftRequest, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(ClearCampaignDraftRequest.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(ClearCampaignDraftRequest.DraftId));
    }

    draft.ClearCampaign();

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}



