namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

/// <summary>
/// Tuning for <see cref="PodcastEpisodeMatcher"/>.
/// </summary>
/// <param name="ToleranceDays">How many days an episode may differ from the release date.</param>
/// <param name="MinTitleScore">Lowest title score (0 to 1) a title-ranked match may have.</param>
/// <param name="MinScoreMargin">How far the best title score must beat the runner-up.</param>
public sealed record PodcastMatchSettings(
  int ToleranceDays = 2,
  double MinTitleScore = 0.6,
  double MinScoreMargin = 0.2
);
