namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

internal sealed record UpdateGuestDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public string? Title { get; init; }
  public DateOnly? DraftDate { get; init; }
  public string? Type { get; init; }
  public int? NumberOfPicks { get; init; }
  public IReadOnlyList<UpdateGuestDraftPositionInput> Positions { get; init; } = [];
}
