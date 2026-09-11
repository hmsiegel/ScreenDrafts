namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.RevealPick;

internal sealed record RevealGuestDraftPickRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
