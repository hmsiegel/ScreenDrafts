namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.UpdateDraft;

internal sealed record UpdateDraftCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public string? Title { get; init; }
  public DateOnly? DraftDate { get; init; }
  public string? Type { get; init; }
  public int? NumberOfPicks { get; init; }
  public IReadOnlyList<GuestDraftPositionInput> Positions { get; init; } = [];
}
