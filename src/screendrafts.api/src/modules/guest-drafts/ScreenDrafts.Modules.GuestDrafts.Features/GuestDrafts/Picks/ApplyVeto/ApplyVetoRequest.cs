namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed record ApplyVetoRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  public string? Note { get; init; }
}
