namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Create;

internal sealed record CreateGuestDraftRequest
{
  public required string Title { get; init; }
  public required string Type { get; init; }
  public DateOnly? DraftDate { get; init; }
  public required int NumberOfPicks { get; init; }
  public IReadOnlyList<GuestDraftPositionInput> Positions { get; init; } = [];
}
