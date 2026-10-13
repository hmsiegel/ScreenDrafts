namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>A title that has finished at three of the four places of The Cycle and lacks the fourth.</summary>
internal sealed record TitleNearMissEntry
{
  public required string MediaPublicId { get; init; }
  public required string Title { get; init; }
  public required int AppearanceCount { get; init; }

  /// <summary>The three places from 1 to 4 the title has finished at, lowest first.</summary>
  public IReadOnlyList<int> PositionsHeld { get; init; } = [];

  /// <summary>The place from 1 to 4 the title still needs.</summary>
  public required int MissingPosition { get; init; }
}
