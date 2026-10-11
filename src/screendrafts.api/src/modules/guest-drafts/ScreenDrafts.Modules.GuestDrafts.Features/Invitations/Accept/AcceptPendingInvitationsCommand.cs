namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Accept;

// ── Command ───────────────────────────────────────────────────────────────────
internal sealed record AcceptPendingInvitationsCommand : ICommand
{
  public required Guid UserId { get; init; }
  public required string Email { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}
