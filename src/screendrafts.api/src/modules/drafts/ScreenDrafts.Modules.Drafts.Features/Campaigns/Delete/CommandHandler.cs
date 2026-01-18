using ScreenDrafts.Modules.Drafts.Features.Categories.Delete;
using ScreenDrafts.Modules.Drafts.Features.Series.Delete;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

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

    if (campaign.IsDeleted)
    {
      return Result.Success();
    }

    var deleteResult = campaign.SoftDelete();

    if (!deleteResult.IsFailure)
    {
      return Result.Failure(CampaignErrors.DeletionFailed(request.PublicId));
    }

    return Result.Success();
  }
}
