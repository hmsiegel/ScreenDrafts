namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed record SetCustomPositionsCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required IReadOnlyList<PositionInput> Positions { get; init; }
}
