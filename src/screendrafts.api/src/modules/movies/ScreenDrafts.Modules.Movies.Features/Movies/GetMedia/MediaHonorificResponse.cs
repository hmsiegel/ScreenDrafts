namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record MediaHonorificResponse
{
  public required int AppearanceHonorificValue { get; init; }
  public required string AppearanceHonorificName { get; init; }
  public required int PositionHonorificValue { get; init; }
  public required int AppearanceCount { get; init; }
  public required bool IsUnifiedNo1 { get; init; }
  public required bool IsTheCycle { get; init; }
}
