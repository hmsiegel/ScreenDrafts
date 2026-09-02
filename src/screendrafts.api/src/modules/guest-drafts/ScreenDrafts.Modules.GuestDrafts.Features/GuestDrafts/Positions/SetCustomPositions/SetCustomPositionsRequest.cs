namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed record SetCustomPositionsRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required IReadOnlyList<PositionInput> Positions { get; init; }
}
