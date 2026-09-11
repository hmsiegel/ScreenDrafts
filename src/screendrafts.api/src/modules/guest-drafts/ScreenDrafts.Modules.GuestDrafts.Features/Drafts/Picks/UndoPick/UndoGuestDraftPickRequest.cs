namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.UndoPick;

internal sealed record UndoGuestDraftPickRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
