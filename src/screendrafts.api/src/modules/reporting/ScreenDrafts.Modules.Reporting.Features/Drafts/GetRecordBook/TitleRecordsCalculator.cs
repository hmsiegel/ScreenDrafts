using ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>
/// Builds the Title section of the Record Book: the most-drafted records, and the honorific-gap records.
/// A gap is counted in main-feed episodes between the release dates of two appearances, counting every released part as
/// an episode, the same way the title lists do. Both appearances must have a main-feed release.
/// </summary>
internal static class TitleRecordsCalculator
{
  // Appearance indexes are zero-based: 0 is a title's first appearance, 1 its Marquee of Fame join, 2 its Hat Trick join
  // and 3 its Grand Slam join.
  private static readonly GapSpec[] _gaps =
  [
    new("join-mof", "join the Marquee of Fame", 0, 1),
    new("mof-hat-trick", "Marquee of Fame to Hat Trick", 1, 2),
    new("first-hat-trick", "1st appearance to Hat Trick", 0, 2),
    new("mof-grand-slam", "Marquee of Fame to Grand Slam", 1, 3),
    new("first-grand-slam", "1st appearance to Grand Slam", 0, 3),
    new("hat-trick-grand-slam", "Hat Trick to Grand Slam", 2, 3),
  ];

  public static RecordBookSection Build(
    IReadOnlyList<MediaRow> media,
    IReadOnlyList<TitleAppearanceRow> appearances
  )
  {
    ArgumentNullException.ThrowIfNull(media);
    ArgumentNullException.ThrowIfNull(appearances);

    var records = new List<RecordItem>();

    AddIfNotNull(
      records,
      RecordRanker.Build(
        "title.most-times-drafted",
        "Most times drafted",
        RecordRanker.Count,
        media,
        m => m.TimesDrafted,
        true,
        Holder
      )
    );

    AddIfNotNull(
      records,
      RecordRanker.Build(
        "title.most-times-drafted-no1",
        "Most times drafted No. 1",
        RecordRanker.Count,
        media,
        m => m.TimesDraftedNo1,
        true,
        Holder
      )
    );

    return new RecordBookSection
    {
      Key = "title",
      Title = "Title Records",
      Groups =
      [
        new RecordBookGroup
        {
          Key = "drafts",
          Title = "Drafts",
          Records = records,
        },
        new RecordBookGroup
        {
          Key = "honorifics",
          Title = "Honorific Records",
          Records = BuildGapRecords(appearances),
        },
      ],
    };
  }

  private static List<RecordItem> BuildGapRecords(IReadOnlyList<TitleAppearanceRow> appearances)
  {
    var timelines = appearances
      .GroupBy(a => a.MediaPublicId)
      .Select(g => TitleHonorificEngine.OrderAppearances(g))
      .ToList();

    var records = new List<RecordItem>();

    foreach (var spec in _gaps)
    {
      var gaps = timelines.Select(t => Gap(t, spec)).OfType<TitleGap>().ToList();

      AddIfNotNull(
        records,
        RecordRanker.Build(
          $"title.gap.{spec.Code}.shortest",
          $"Shortest gap to {spec.Label} (episodes)",
          RecordRanker.Count,
          gaps,
          g => g.Episodes,
          false,
          HolderFor
        )
      );

      AddIfNotNull(
        records,
        RecordRanker.Build(
          $"title.gap.{spec.Code}.longest",
          $"Longest gap to {spec.Label} (episodes)",
          RecordRanker.Count,
          gaps,
          g => g.Episodes,
          true,
          HolderFor
        )
      );
    }

    return records;
  }

  /// <summary>Null when the title has not reached the later appearance, or either appearance has no main-feed release.</summary>
  private static TitleGap? Gap(List<TitleAppearanceRow> timeline, GapSpec spec)
  {
    if (timeline.Count <= spec.ToIndex)
    {
      return null;
    }

    var earlier = timeline[spec.FromIndex];
    var later = timeline[spec.ToIndex];

    if (earlier.ReleaseRank is not { } earlierRank || later.ReleaseRank is not { } laterRank)
    {
      return null;
    }

    return new TitleGap(
      earlier.MediaPublicId,
      earlier.MediaTitle,
      laterRank - earlierRank,
      TitleHonorificEngine.DaysBetween(earlier.ReleasedOn, later.ReleasedOn),
      earlier,
      later
    );
  }

  private static RecordHolder HolderFor(TitleGap g)
  {
    var span = $"{Where(g.Earlier)} to {Where(g.Later)}";

    string context;
    if (g.Days is { } days)
    {
      var dayWord = days == 1 ? "day" : "days";
      context = $"{span} · {days.ToString("N0", CultureInfo.InvariantCulture)} {dayWord}";
    }
    else
    {
      context = span;
    }

    return new RecordHolder
    {
      Kind = "title",
      Name = g.Title,
      PublicId = g.MediaPublicId,
      Context = context,
    };
  }

  private static string Where(TitleAppearanceRow a) =>
    a.EpisodeNumber is { } episode
      ? $"Ep. {episode.ToString(CultureInfo.InvariantCulture)}"
      : a.DraftTitle;

  private static RecordHolder Holder(MediaRow m) =>
    new()
    {
      Kind = "title",
      Name = m.MediaTitle,
      PublicId = m.MediaPublicId,
    };

  private static void AddIfNotNull(List<RecordItem> records, RecordItem? item)
  {
    if (item is not null)
    {
      records.Add(item);
    }
  }

  private sealed record GapSpec(string Code, string Label, int FromIndex, int ToIndex);

  private sealed record TitleGap(
    string MediaPublicId,
    string Title,
    int Episodes,
    int? Days,
    TitleAppearanceRow Earlier,
    TitleAppearanceRow Later
  );
}
