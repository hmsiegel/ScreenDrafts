namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftParticipant : Entity<GuestDraftParticipantId>
{
  private GuestDraftParticipant(
    string publicId,
    GuestDraftId guestDraftId,
    Guid userId,
    bool isOwner,
    DateTime joinedOnUtc,
    GuestDraftParticipantId? id = null
  )
    : base(id ?? GuestDraftParticipantId.CreateUnique())
  {
    PublicId = publicId;
    GuestDraftId = guestDraftId;
    UserId = userId;
    IsOwner = isOwner;
    JoinedOnUtc = joinedOnUtc;
  }

  private GuestDraftParticipant() { }

  public string PublicId { get; private set; } = default!;
  public GuestDraftId GuestDraftId { get; private set; } = default!;
  public Guid UserId { get; private set; }
  public bool IsOwner { get; private set; }
  public DateTime JoinedOnUtc { get; private set; }
  public GuestDraft GuestDraft { get; private set; } = default!;

  // internal: participants are only ever created through GuestDraft.Create /
  // GuestDraft.InviteParticipant, never constructed directly by a handler.
  internal static GuestDraftParticipant Create(
    string publicId,
    GuestDraftId guestDraftId,
    Guid userId,
    bool isOwner
  )
  {
    return new GuestDraftParticipant(
      publicId: publicId,
      guestDraftId: guestDraftId,
      userId: userId,
      isOwner: isOwner,
      joinedOnUtc: DateTime.UtcNow
    );
  }
}
