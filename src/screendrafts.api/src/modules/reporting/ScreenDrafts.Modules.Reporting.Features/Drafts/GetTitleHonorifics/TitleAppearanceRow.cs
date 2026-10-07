namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>One canonical appearance of a title: the landed pick of that title in one draft part.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Dapper.")]
internal sealed class TitleAppearanceRow
{
  public string MediaPublicId { get; set; } = string.Empty;
  public string MediaTitle { get; set; } = string.Empty;
  public Guid DraftId { get; set; }
  public string DraftPublicId { get; set; } = string.Empty;
  public string DraftTitle { get; set; } = string.Empty;
  public string PartPublicId { get; set; } = string.Empty;
  public int PartIndex { get; set; }
  public int SubDraftIndex { get; set; }
  public int Position { get; set; }

  /// <summary>Order the pick was played in its part. Orders titles that join in the same episode.</summary>
  public int PlayOrder { get; set; }

  public int? EpisodeNumber { get; set; }

  /// <summary>Parts in the draft. A multi-part draft shows "Part 4" next to its shared episode number.</summary>
  public int TotalParts { get; set; }

  /// <summary>
  /// Position of this part among ALL main-feed part releases, ordered by release date. Counts every part as an episode,
  /// as the home page's "episodes produced" does. Null when the part has no main-feed release.
  /// </summary>
  public int? ReleaseRank { get; set; }

  /// <summary>Main-feed release date of this part as yyyy-MM-dd, or null when the part has none.</summary>
  public string? ReleasedOn { get; set; }
}
