namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

public sealed record PickItem
{
  public int Position { get; init; }
  public int PlayOrder { get; init; }
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public string? MovieVersionName { get; init; }
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverridden { get; init; }
  public string? VetoedByPublicId { get; init; }
  public string? VetoedByDisplayName { get; init; }
  public string? OverrideByPublicId { get; init; }
  public string? OverrideByDisplayName { get; init; }
}
