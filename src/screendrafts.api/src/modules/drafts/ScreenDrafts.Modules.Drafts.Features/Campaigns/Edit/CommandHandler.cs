namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class CommandHandler(ICampaignsRepository campaignRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ICampaignsRepository _campaignRepository = campaignRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var campaign = await _campaignRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(request.PublicId));
    }

    if (request.Name is not null)
    {
      var renameResult = campaign.Rename(request.Name);
      if (renameResult.IsFailure)
      {
        return renameResult;
      }
    }

    if (request.Slug is not null)
    {
      var changeSlugResult = campaign.ChangeSlug(request.Slug);
      if (changeSlugResult.IsFailure)
      {
        return changeSlugResult;
      }
    }

    _campaignRepository.Update(campaign);

    return Result.Success();
  }
}
