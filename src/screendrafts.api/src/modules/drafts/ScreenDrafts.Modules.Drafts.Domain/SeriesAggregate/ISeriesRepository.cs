namespace ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

public interface ISeriesRepository : IRepository<Series, SeriesId>
{
  Task<bool> ExistsByPublicIdAsync(string? seriesPublicId, CancellationToken cancellationToken);
  Task<Series?> GetSeriesByPublicIdIncludingDeletedAsync(
    string? seriesPublicId,
    CancellationToken cancellationToken
  );
}
