namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface ISeriesRepository : IRepository<Series, SeriesId>
{
  Task<Series?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
}
