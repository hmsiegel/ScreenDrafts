namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;

public sealed record SpotlightPickResponse
{
  public int Position { get; init; }
  public string MediaPublicId { get; init; } = default!;
  public string MediaTitle { get; init; } = default!;
}
