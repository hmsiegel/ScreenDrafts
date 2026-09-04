namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoVeto;

internal sealed record UndoGuestDraftVetoRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
