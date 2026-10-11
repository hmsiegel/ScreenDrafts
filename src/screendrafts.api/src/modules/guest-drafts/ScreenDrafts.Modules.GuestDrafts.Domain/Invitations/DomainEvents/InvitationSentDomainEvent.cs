namespace ScreenDrafts.Modules.GuestDrafts.Domain.Invitations.DomainEvents;

/// <summary>
/// Raised when an invitation is created or resent. A Features handler turns it
/// into an integration event so the email goes out only after the transaction commits.
/// </summary>
public sealed class InvitationSentDomainEvent(
  Guid invitationId,
  string draftPublicId,
  Guid invitedByUserId,
  string email,
  string name
) : DomainEvent
{
  public Guid InvitationId { get; init; } = invitationId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public Guid InvitedByUserId { get; init; } = invitedByUserId;
  public string Email { get; init; } = email;
  public string Name { get; init; } = name;
}
