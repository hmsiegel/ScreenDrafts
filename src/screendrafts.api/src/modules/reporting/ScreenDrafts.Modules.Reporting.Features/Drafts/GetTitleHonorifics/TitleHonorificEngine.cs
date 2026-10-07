namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// Builds a level's list from the appearance rows. A title's appearances are ordered by the release date of the part
/// each was drafted in, then episode, part, sub-draft and the order the pick was played, so titles that join on the
/// same part are numbered in play order. The title joins a level on the appearance that reaches the level's minimum.
/// </summary>
internal static class TitleHonorificEngine
{
  public static TitleHonorificResult Run(
    IReadOnlyList<TitleAppearanceRow> rows,
    TitleHonorificSpec spec
  )
  {
    ArgumentNullException.ThrowIfNull(rows);
    ArgumentNullException.ThrowIfNull(spec);

    var timelines = rows.GroupBy(r => r.MediaPublicId)
      .Select(g => new TitleTimeline(g.Key, g.First().MediaTitle, OrderAppearances(g)))
      .ToList();

    var counts = TitleHonorificLevels
      .All.Select(level => new TitleHonorificCount
      {
        Code = level.Code,
        Label = level.Label,
        Count = timelines.Count(t => t.Appearances.Count >= level.MinAppearances),
      })
      .ToList();

    var numbered = Number(timelines, spec.Level);
    var matching = Filter(numbered, spec.Search);
    var sorted = Sort(matching, spec.Sort);

    var page = sorted.Skip((spec.Page - 1) * spec.PageSize).Take(spec.PageSize).ToList();

    return new TitleHonorificResult(page, sorted.Count, counts);
  }

  /// <summary>
  /// Chronological order: the part's main-feed release date, then the episode number, then part, sub-draft and the
  /// order the pick was played. The release date leads because the parts of a multi-part draft come out on different
  /// days, and other drafts can be released between them. A part with no release date sorts last.
  /// </summary>
  private static (int Day, int Episode, int Part, int SubDraft, int Play) SortKey(
    TitleAppearanceRow a
  ) =>
    (
      ReleaseDay(a.ReleasedOn),
      a.EpisodeNumber ?? int.MaxValue,
      a.PartIndex,
      a.SubDraftIndex,
      a.PlayOrder
    );

  private static int ReleaseDay(string? releasedOn) =>
    DateOnly.TryParseExact(
      releasedOn,
      "yyyy-MM-dd",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out var date
    )
      ? date.DayNumber
      : int.MaxValue;

  private static List<TitleAppearanceRow> OrderAppearances(
    IEnumerable<TitleAppearanceRow> appearances
  ) => [.. appearances.OrderBy(SortKey)];

  private static List<TitleHonorificEntry> Number(
    List<TitleTimeline> timelines,
    TitleHonorificLevel level
  )
  {
    var joined = timelines
      .Where(t => t.Appearances.Count >= level.MinAppearances)
      .Select(t => (Timeline: t, Join: t.Appearances[level.MinAppearances - 1]))
      .OrderBy(x => SortKey(x.Join))
      .ThenBy(x => x.Timeline.Title, StringComparer.OrdinalIgnoreCase)
      .ToList();

    return [.. joined.Select((x, index) => ToEntry(index + 1, x.Timeline, x.Join))];
  }

  private static TitleHonorificEntry ToEntry(
    int number,
    TitleTimeline timeline,
    TitleAppearanceRow join
  )
  {
    var first = timeline.Appearances[0];

    return new TitleHonorificEntry
    {
      Number = number,
      MediaPublicId = timeline.MediaPublicId,
      Title = timeline.Title,
      AppearanceCount = timeline.Appearances.Count,
      JoinedDraftTitle = join.DraftTitle,
      JoinedDraftPublicId = join.DraftPublicId,
      JoinedEpisode = join.EpisodeNumber,
      JoinedPartIndex = join.PartIndex,
      JoinedTotalParts = join.TotalParts,
      JoinedOn = join.ReleasedOn,
      FirstDraftTitle = first.DraftTitle,
      FirstEpisode = first.EpisodeNumber,
      FirstOn = first.ReleasedOn,
      GapEpisodes = join.ReleaseRank - first.ReleaseRank,
      GapDays = DaysBetween(first.ReleasedOn, join.ReleasedOn),
      Appearances =
      [
        .. timeline.Appearances.Select(
          (a, index) =>
            new TitleAppearanceEntry
            {
              AppearanceNumber = index + 1,
              DraftTitle = a.DraftTitle,
              DraftPublicId = a.DraftPublicId,
              EpisodeNumber = a.EpisodeNumber,
              PartIndex = a.PartIndex,
              TotalParts = a.TotalParts,
              ReleasedOn = a.ReleasedOn,
              Position = a.Position,
            }
        ),
      ],
    };
  }

  private static int? DaysBetween(string? from, string? to)
  {
    if (
      !DateOnly.TryParseExact(
        from,
        "yyyy-MM-dd",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var start
      )
      || !DateOnly.TryParseExact(
        to,
        "yyyy-MM-dd",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var end
      )
    )
    {
      return null;
    }

    return end.DayNumber - start.DayNumber;
  }

  private static List<TitleHonorificEntry> Filter(List<TitleHonorificEntry> entries, string? search)
  {
    if (string.IsNullOrWhiteSpace(search))
    {
      return entries;
    }

    var term = search.Trim();

    return [.. entries.Where(e => e.Title.Contains(term, StringComparison.OrdinalIgnoreCase))];
  }

  private static List<TitleHonorificEntry> Sort(List<TitleHonorificEntry> entries, string sort) =>
    sort switch
    {
      TitleHonorificSorts.Oldest => [.. entries.OrderBy(e => e.Number)],
      TitleHonorificSorts.Alphabetical =>
      [
        .. entries.OrderBy(e => e.Title, StringComparer.OrdinalIgnoreCase),
      ],
      TitleHonorificSorts.Appearances =>
      [
        .. entries.OrderByDescending(e => e.AppearanceCount).ThenBy(e => e.Number),
      ],
      _ => [.. entries.OrderByDescending(e => e.Number)],
    };

  private sealed record TitleTimeline(
    string MediaPublicId,
    string Title,
    List<TitleAppearanceRow> Appearances
  );
}
