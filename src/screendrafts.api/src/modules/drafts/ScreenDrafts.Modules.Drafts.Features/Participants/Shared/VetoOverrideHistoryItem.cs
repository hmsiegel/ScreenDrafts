namespace ScreenDrafts.Modules.Drafts.Features.Participants.Shared;

/// <summary>A veto this drafter overrode: whose veto, on which pick, played by whom.</summary>
internal sealed record VetoOverrideHistoryItem
{
  public required DraftBrief Draft { get; init; }
  public required string TargetPickPublicId { get; init; }
  public int Position { get; init; }
  public DateOnly? ReleaseDate { get; init; }
  public int PartIndex { get; init; }
  public int SubDraftIndex { get; init; }
  public int PlayOrder { get; init; }
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public string? PickPlayedByPublicId { get; init; }
  public string? PickPlayedByDisplayName { get; init; }
  public string? VetoedByPublicId { get; init; }
  public string? VetoedByDisplayName { get; init; }
}
