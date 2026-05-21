namespace ScreenDrafts.Modules.Drafts.PublicApi;

public sealed record MediaAppearanceRecord
{
  public string DraftPublicId { get; init; } = string.Empty;
  public string DraftTitle { get; init; } = string.Empty;
  public int? EpisodeNumber { get; init; }
  public string PickedByDisplayName { get; init; } = string.Empty;
  public string? PickedByPersonPublicId { get; init; }
  public int? Position { get; init; }
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverride { get; init; }
  public string? VetoedByDisplayName { get; init; }
  public string? VetoOverrideByDisplayName { get; init; }
  public bool IsPatreon { get; init; }
}
