namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.UpdateDraft;

internal sealed record UpdateGuestDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public string? Title { get; init; }
  public DateOnly? DraftDate { get; init; }
  public string? Type { get; init; }
  public int? NumberOfPicks { get; init; }
  public IReadOnlyList<GuestDraftPositionInput> Positions { get; init; } = [];
}
