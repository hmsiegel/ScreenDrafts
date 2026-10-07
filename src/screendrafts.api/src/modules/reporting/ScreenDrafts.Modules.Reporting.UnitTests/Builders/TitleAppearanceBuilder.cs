namespace ScreenDrafts.Modules.Reporting.UnitTests.Builders;

internal static class TitleAppearanceBuilder
{
  /// <summary>One landed appearance of a title in a draft part.</summary>
  public static TitleAppearanceRow Appearance(
    string title,
    string draft,
    int? episode,
    string? releasedOn,
    int? releaseRank,
    int play = 1,
    int part = 1,
    int totalParts = 1,
    int subDraft = 0) =>
    new()
    {
      MediaPublicId = $"m_{title}",
      MediaTitle = title,
      DraftId = Guid.Empty,
      DraftPublicId = $"d_{draft}",
      DraftTitle = draft,
      PartPublicId = $"dp_{draft}_{part}",
      PartIndex = part,
      TotalParts = totalParts,
      SubDraftIndex = subDraft,
      Position = play,
      PlayOrder = play,
      EpisodeNumber = episode,
      ReleasedOn = releasedOn,
      ReleaseRank = releaseRank,
    };

  public static TitleHonorificSpec Spec(
    string level = TitleHonorificLevels.MarqueeOfFame,
    string? search = null,
    string sort = TitleHonorificSorts.Newest,
    int page = 1,
    int pageSize = 50) =>
    new(TitleHonorificLevels.Find(level)!, search, sort, page, pageSize);
}
