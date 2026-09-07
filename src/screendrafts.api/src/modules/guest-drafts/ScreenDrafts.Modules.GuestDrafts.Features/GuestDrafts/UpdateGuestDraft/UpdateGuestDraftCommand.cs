namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

internal sealed record UpdateGuestDraftCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public string? Title { get; init; }
  public DateOnly? DraftDate { get; init; }
  public string? Type { get; init; }
  public int? NumberOfPicks { get; init; }
  public IReadOnlyList<UpdateGuestDraftPositionInput> Positions { get; init; } = [];
}
