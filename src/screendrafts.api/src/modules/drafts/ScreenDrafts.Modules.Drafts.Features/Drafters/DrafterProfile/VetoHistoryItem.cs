namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

public sealed record VetoHistoryItem
{
  public required DraftBrief Draft { get; init; }
  public required string TargetPickPublicId { get; init; }
  public int Position { get; init; }
  public int PlayOrder { get; init; }
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public required string TargetDrafterPublicId { get; init; }
  public required string TargetDrafterDisplayName { get; init; }
  public bool WasVetoOverridden { get; init; }
  public string? OverrideByPublicId { get; init; }
  public string? OverrideByDisplayName { get; init; }
}
