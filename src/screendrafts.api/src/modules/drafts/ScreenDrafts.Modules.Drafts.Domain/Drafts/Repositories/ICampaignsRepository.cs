namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICampaignsRepository
{
  void Add(Campaign campaign);

  void Update(Campaign campaign);

  void Delete(Campaign campaign);

  Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken);

  Task<Campaign?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  bool ExistsBySlug(string slug);
}
