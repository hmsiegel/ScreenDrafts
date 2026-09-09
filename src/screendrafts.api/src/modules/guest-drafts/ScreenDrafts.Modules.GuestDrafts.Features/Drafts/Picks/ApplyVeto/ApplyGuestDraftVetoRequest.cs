namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVeto;

internal sealed record ApplyGuestDraftVetoRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  public string? Note { get; init; }
}
