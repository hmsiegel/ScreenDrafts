namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCampaignToDraft;

public sealed record AddCampaignToDraftCommand(Guid DraftId, Guid CampaignId) : ICommand;

internal sealed class AddCampaignToDraftCommandValidator : AbstractValidator<AddCampaignToDraftCommand>
{
  public AddCampaignToDraftCommandValidator()
  {
    // Add validation rules here if needed in the future
  }
}

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
  }
}

