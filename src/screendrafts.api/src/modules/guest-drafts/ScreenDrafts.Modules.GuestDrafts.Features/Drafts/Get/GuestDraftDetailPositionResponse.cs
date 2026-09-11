namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed record GuestDraftDetailPositionResponse
{
  public required string Name { get; init; }
  public IReadOnlyList<int> Picks { get; init; } = [];
  public string? AssignedParticipantDisplayName { get; init; }
}
