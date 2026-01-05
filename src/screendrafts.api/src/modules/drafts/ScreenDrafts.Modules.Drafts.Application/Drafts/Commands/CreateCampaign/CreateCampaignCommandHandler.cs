namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateCampaign;

internal sealed class CreateCampaignCommandHandler(ICampaignRepository campaignRepository)
  : ICommandHandler<CreateCampaignCommand, Guid>
{
  private readonly ICampaignRepository _campaignRepository = campaignRepository;

  public Task<Result<Guid>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
  {
    var campaign = new Campaign(request.Slug, request.Name);
   
    _campaignRepository.Add(campaign);
    
    return Task.FromResult(Result.Success(campaign.Id));
  }
}
