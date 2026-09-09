namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

/// <summary>
/// A board position and its pick slots. AssignedParticipantId is the raw
/// GuestDraftParticipant id, not the GuestDrafter's own PublicId -- look the
/// participant up in Participants by this id if the PublicId is needed.
/// </summary>
internal sealed record GuestDraftGameplayPositionResponse
{
  public string PositionPublicId { get; init; } = default!;
  public string Name { get; init; } = default!;
  public int[] Picks { get; init; } = [];
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public bool HasBonusFungibleToken { get; init; }
  public Guid? AssignedParticipantId { get; init; }
  public string? AssignedParticipantDisplayName { get; init; }
}
