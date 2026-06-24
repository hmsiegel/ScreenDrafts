namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftPartRequest
{
  public required int PartIndex { get; init; }
  public required int MinimumPosition { get; init; }
  public required int MaximumPosition { get; init; }
  public CreateDraftCommunityRequest? Community { get; init; }
  public IReadOnlyList<CreateDraftPositionRequest> Positions { get; init; } = [];
}
