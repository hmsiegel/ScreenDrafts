namespace ScreenDrafts.Modules.Drafts.Domain.Campaigns;
public interface ICampaignRepository : IRepository<Campaign>
{
  Task<Campaign?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  bool ExistsBySlug(string slug);
  Task<bool> ExistsByPublicIdAsync(string? campaignPublicId, CancellationToken cancellationToken);
}
