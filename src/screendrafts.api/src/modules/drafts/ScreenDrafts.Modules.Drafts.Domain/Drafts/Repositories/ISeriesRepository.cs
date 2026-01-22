namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface ISeriesRepository : IRepository<Series, SeriesId>
{
  Task<bool> ExistsByPublicIdAsync(string? seriesPublicId, CancellationToken cancellationToken);
  Task<Series?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
}
