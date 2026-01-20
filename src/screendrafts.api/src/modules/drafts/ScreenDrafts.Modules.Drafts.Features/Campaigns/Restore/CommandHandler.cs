namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class CommandHandler(ICampaignsRepository campaignsRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ICampaignsRepository _campaignsRepository = campaignsRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var campaign = await _campaignsRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(request.PublicId));
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
