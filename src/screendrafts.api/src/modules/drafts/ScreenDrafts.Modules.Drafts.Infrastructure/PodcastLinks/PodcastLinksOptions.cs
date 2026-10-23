namespace ScreenDrafts.Modules.Drafts.Infrastructure.PodcastLinks;

internal sealed class PodcastLinksOptions
{
  public int IntervalInSeconds { get; init; } = 900;
  public int BatchSize { get; init; } = 100;
  public int ToleranceDays { get; init; } = 2;
  public double MinTitleScore { get; init; } = 0.6;
  public double MinScoreMargin { get; init; } = 0.2;

  /// <summary>
  /// Log outcomes; write nothing.
  /// </summary>
  public bool DryRun { get; init; }
}
