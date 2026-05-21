namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record MediaAppearanceResponse
{
  public required string DraftPublicId { get; init; }
  public required string DraftTitle { get; init; }
  public int? EpisodeNumber { get; init; }
  public required string PickedByDisplayName { get; init; }
  public string? PickedByPersonPublicId { get; init; }
  public int? Position { get; init; }
  public required bool WasVetoed { get; init; }
  public required bool WasVetoOverridden { get; init; }
  public required bool WasCommissionerOverride { get; init; }
  public string? VetoedByDisplayName { get; init; }
  public string? VetoOverrideByDisplayName { get; init; }
  public required bool IsPatreon { get; init; }
}
