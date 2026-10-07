namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleAppearanceEntry
{
  /// <summary>1 for the title's first canonical appearance, 2 for the second, and so on.</summary>
  public required int AppearanceNumber { get; init; }
  public required string DraftTitle { get; init; }
  public required string DraftPublicId { get; init; }
  public int? EpisodeNumber { get; init; }

  /// <summary>Which part of the draft this appearance was in.</summary>
  public int PartIndex { get; init; }

  /// <summary>Parts in the draft. Show the part only when there is more than one.</summary>
  public int TotalParts { get; init; }

  /// <summary>Main-feed release date, yyyy-MM-dd.</summary>
  public string? ReleasedOn { get; init; }
  public required int Position { get; init; }
}
