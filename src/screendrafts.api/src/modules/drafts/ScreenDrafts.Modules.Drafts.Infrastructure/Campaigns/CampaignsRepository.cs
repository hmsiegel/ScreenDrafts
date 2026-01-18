namespace ScreenDrafts.Modules.Drafts.Infrastructure.Campaigns;

internal sealed class CampaignsRepository(DraftsDbContext dbContext) : ICampaignsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Campaign campaign)
  {
    _dbContext.Campaigns.Add(campaign);
  }

  public void Delete(Campaign campaign)
  {
    _dbContext.Campaigns.Remove(campaign);
  }

  public bool ExistsBySlug(string slug)
  {
    return _dbContext.Campaigns.Any(c => c.Slug == slug);
  }

  public Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken)
  {
    return _dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);
  }

  public Task<Campaign?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Campaigns.FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);
  }

  public void Update(Campaign campaign)
  {
    _dbContext.Campaigns.Update(campaign);
  }
}
