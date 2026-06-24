namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftPartInput
{
  public required int PartIndex { get; init; }
  public required int MinimumPosition { get; init; }
  public required int MaximumPosition { get; init; }
  public CommunityInput? Community { get; init; }
  public IReadOnlyList<DraftPositionInput> Positions { get; init; } = [];
}
