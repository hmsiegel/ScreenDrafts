namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICampaignRepository
{
  void Add(Campaign campaign);

  void Update(Campaign campaign);

  void Delete(Campaign campaign);

  Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken);
}
