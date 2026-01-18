namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICampaignsRepository : IRepository<Campaign>
{
  Task<Campaign?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  bool ExistsBySlug(string slug);
}
