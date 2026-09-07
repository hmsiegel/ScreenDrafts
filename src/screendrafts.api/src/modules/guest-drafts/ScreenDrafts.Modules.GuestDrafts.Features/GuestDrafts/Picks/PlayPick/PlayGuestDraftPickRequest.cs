namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed record PlayGuestDraftPickRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required string MoviePublicId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
}
