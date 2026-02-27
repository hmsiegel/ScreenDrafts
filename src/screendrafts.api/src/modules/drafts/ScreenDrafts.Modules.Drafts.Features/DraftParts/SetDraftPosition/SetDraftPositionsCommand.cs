namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetDraftPosition;

internal sealed record SetDraftPositionsCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public IReadOnlyList<DraftPositionRequest> Positions { get; init; } = [];
}
