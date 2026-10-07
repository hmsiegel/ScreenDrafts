namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record GetTitleHonorificsResponse
{
  public required string Level { get; init; }
  public required string LevelLabel { get; init; }
  public required int MinAppearances { get; init; }
  public required bool IncludesNonCanonical { get; init; }

  public required int Page { get; init; }
  public required int PageSize { get; init; }
  public required int TotalPages { get; init; }

  /// <summary>Titles matching the search at this level.</summary>
  public required int TotalMatching { get; init; }

  /// <summary>Counts for every level, for the sub-tab labels.</summary>
  public IReadOnlyList<TitleHonorificCount> Counts { get; init; } = [];

  public IReadOnlyList<TitleHonorificEntry> Titles { get; init; } = [];
}
