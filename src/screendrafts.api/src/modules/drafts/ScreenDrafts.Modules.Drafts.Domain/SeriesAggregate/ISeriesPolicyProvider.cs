namespace ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

/// <summary>
/// Infrastructure service for loading Series by ID.
/// Policy computation lives on Series itself and SeriesPolicyRules.
/// This interface only exists because handlers sometimes need Series
/// without havin loaded a Draft that navigates to it.
/// </summary>
public interface ISeriesPolicyProvider
{
  Task<Series?> GetSeriesAsyc(SeriesId seriesId, CancellationToken cancellationToken);
}
