namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleHonorificEntry
{
  /// <summary>Join order at this level: 1 is the first title to join, 97 the ninety-seventh.</summary>
  public required int Number { get; init; }

  public required string MediaPublicId { get; init; }
  public required string Title { get; init; }
  public required int AppearanceCount { get; init; }

  public required string JoinedDraftTitle { get; init; }
  public required string JoinedDraftPublicId { get; init; }
  public int? JoinedEpisode { get; init; }
  public int JoinedPartIndex { get; init; }
  public int JoinedTotalParts { get; init; }
  public string? JoinedOn { get; init; }

  public required string FirstDraftTitle { get; init; }
  public int? FirstEpisode { get; init; }
  public string? FirstOn { get; init; }

  /// <summary>
  /// Main-feed episodes between the first appearance and the one that joined the level, counting every released part as
  /// an episode. 1 means the very next release.
  /// </summary>
  public int? GapEpisodes { get; init; }

  /// <summary>Days between the two main-feed release dates.</summary>
  public int? GapDays { get; init; }

  public IReadOnlyList<TitleAppearanceEntry> Appearances { get; init; } = [];
}
