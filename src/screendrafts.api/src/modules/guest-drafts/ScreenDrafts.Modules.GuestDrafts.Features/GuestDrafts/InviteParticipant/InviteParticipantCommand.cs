namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed record InviteParticipantCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required string InviteeUserPublicId { get; init; }
}
